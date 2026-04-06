# ---------------------------------------------------------------
# Virtual Network
# ---------------------------------------------------------------
resource "azurerm_virtual_network" "main" {
  name                = "vnet-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  address_space       = var.vnet_address_space
  tags                = var.tags
}

# ---------------------------------------------------------------
# Subnets
# Cada subnet tiene un proposito especifico para aislar capas.
# ---------------------------------------------------------------

# APIM: unico punto de entrada desde internet
resource "azurerm_subnet" "apim" {
  name                 = "snet-apim"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [var.subnet_apim_prefix]
}

# App Service: delegada para VNet Integration (trafico saliente del App Service)
resource "azurerm_subnet" "appservice" {
  name                 = "snet-appservice"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [var.subnet_appservice_prefix]

  delegation {
    name = "app-service-delegation"
    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

# Private Endpoints: Key Vault, Blob Storage (y Postgres en fase futura)
resource "azurerm_subnet" "private_endpoints" {
  name                 = "snet-private-endpoints"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [var.subnet_private_endpoints_prefix]

  # "Disabled" es necesario para que los Private Endpoints funcionen en esta subnet
  private_endpoint_network_policies = "Disabled"
}

# Database: delegada a PostgreSQL Flexible Server
# Los recursos de esta subnet son manejados por el servicio de Azure
resource "azurerm_subnet" "database" {
  name                 = "snet-database"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [var.subnet_database_prefix]

  delegation {
    name = "postgresql-delegation"
    service_delegation {
      name    = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}

# ---------------------------------------------------------------
# Network Security Groups (NSGs)
# Filtran trafico entrante/saliente por subnet.
# Sin NSGs, la VNet no controla quien puede comunicarse con quien.
# ---------------------------------------------------------------

resource "azurerm_network_security_group" "apim" {
  name                = "nsg-apim-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  tags                = var.tags

  # HTTPS desde internet (trafico de usuarios y del frontend)
  security_rule {
    name                       = "AllowHTTPS"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "Internet"
    destination_address_prefix = "*"
  }

  # Puerto 3443 requerido por Azure para gestionar APIM (health checks, config)
  security_rule {
    name                       = "AllowAPIMManagement"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "3443"
    source_address_prefix      = "ApiManagement"
    destination_address_prefix = "*"
  }
}

resource "azurerm_network_security_group" "appservice" {
  name                = "nsg-appservice-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  tags                = var.tags

  # Solo permite HTTPS desde la subnet de APIM — el App Service no debe ser accesible directamente
  security_rule {
    name                       = "AllowAPIMInbound"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = var.subnet_apim_prefix
    destination_address_prefix = "*"
  }

  # Deniega todo el trafico entrante que no haya sido permitido explicitamente
  security_rule {
    name                       = "DenyAllInbound"
    priority                   = 4096
    direction                  = "Inbound"
    access                     = "Deny"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
}

# NSG para la subnet de private endpoints
# Azure gestiona el trafico hacia los private endpoints internamente;
# el NSG permite auditar y añadir reglas adicionales si se necesita
resource "azurerm_network_security_group" "private_endpoints" {
  name                = "nsg-pe-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  tags                = var.tags

  # Solo trafico desde dentro de la VNet hacia los private endpoints
  security_rule {
    name                       = "AllowVNetInbound"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "DenyAllInbound"
    priority                   = 4096
    direction                  = "Inbound"
    access                     = "Deny"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
}

# ---------------------------------------------------------------
# NSG Associations
# Sin esta asociacion, el NSG existe pero no se aplica a la subnet
# ---------------------------------------------------------------
resource "azurerm_subnet_network_security_group_association" "apim" {
  subnet_id                 = azurerm_subnet.apim.id
  network_security_group_id = azurerm_network_security_group.apim.id
}

resource "azurerm_subnet_network_security_group_association" "appservice" {
  subnet_id                 = azurerm_subnet.appservice.id
  network_security_group_id = azurerm_network_security_group.appservice.id
}

resource "azurerm_subnet_network_security_group_association" "private_endpoints" {
  subnet_id                 = azurerm_subnet.private_endpoints.id
  network_security_group_id = azurerm_network_security_group.private_endpoints.id
}

# ---------------------------------------------------------------
# Private DNS Zones
# Sobrescriben la resolucion DNS publica dentro de la VNet para
# que los nombres de los servicios resuelvan a sus IPs privadas.
# Sin estas zonas, los Private Endpoints no son accesibles.
# ---------------------------------------------------------------

resource "azurerm_private_dns_zone" "postgres" {
  name                = "privatelink.postgres.database.azure.com"
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

resource "azurerm_private_dns_zone" "blob" {
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

resource "azurerm_private_dns_zone" "keyvault" {
  name                = "privatelink.vaultcore.azure.net"
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

# ---------------------------------------------------------------
# DNS Zone Virtual Network Links
# Vinculan las zonas DNS a la VNet para que sean efectivas.
# registration_enabled = false: la zona no registra automaticamente
# los recursos de la VNet, solo resuelve consultas especificas.
# ---------------------------------------------------------------

resource "azurerm_private_dns_zone_virtual_network_link" "postgres" {
  name                  = "link-postgres-${var.environment}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.postgres.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false
  tags                  = var.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "blob" {
  name                  = "link-blob-${var.environment}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.blob.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false
  tags                  = var.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "keyvault" {
  name                  = "link-kv-${var.environment}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.keyvault.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false
  tags                  = var.tags
}
