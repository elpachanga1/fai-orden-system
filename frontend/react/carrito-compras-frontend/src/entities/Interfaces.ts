export interface Product {
    id: number;
    sku: string;
    name: string;
    description?: string;
    availableUnits: number;
    unitPrice: number;
    image?: string;
}

export interface Item {
    id: number;
    name: string;
    idProduct: number;
    quantity: number;
    isDeleted: boolean;
    totalPrice: number;
    productReference: Product;
}

export interface ShoppingCart {
    items: Item[];
    countProducts: number;
    total: number;
}

export interface User {
    id: number;
    username: string;
    name: string;
    password?: string;
    session: Session;
}

interface Session {
    id: number;
    userId: number;
    sessionStart: any;
    sessionEnd: any;
}