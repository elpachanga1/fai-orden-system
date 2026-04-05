import { useEffect, useState } from "react";
import Swal from "sweetalert2";
import { Item, ShoppingCart } from "../entities/Interfaces";
import * as CartService from "../services/CartService";
import * as ShoppingCartUtils from "../utils/ShoppingCartUtils";
import { Counter } from "./Counter";
import { get } from "lodash";
import { getToken } from "../utils/tokenUtil";

export const ShoppingCartMenu = ({ shoppingCart, setShoppingCart }: any) => {
  const [active, setActive] = useState(false);
  const [token, setToken] = useState<string>("");

  useEffect(() => {
    const rawToken = getToken();
    setToken(rawToken);
    const fetchItems = async () => {
      try {
        const shoppingCart: ShoppingCart =
          await ShoppingCartUtils.getShoppingCart(rawToken);
        setShoppingCart(shoppingCart);
      } catch (error) {
        console.error("Error fetching items:", error);
      }
    };

    fetchItems();
  }, [setShoppingCart]);

  const onDeleteProduct = async (item: Item) => {
    await CartService.DeleteProductFromShoppingCart(item.id, token);
    const shoppingCart: ShoppingCart = await ShoppingCartUtils.getShoppingCart(
      token
    );
    setShoppingCart(shoppingCart);
    Swal.fire(
      "Deleted!",
      `Item ${item.id} has been deleted from the shopping cart.`,
      "success"
    );
  };

  const onCleanCart = () => {
    Swal.fire({
      title: "Are you sure?",
      text: "You wont be able to revert this!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "Yes, delete it!",
    }).then(async (result) => {
      if (result.isConfirmed) {
        await CartService.EmptyShoppingCart(token);
        setShoppingCart({
          items: [],
          countProducts: 0,
          total: 0,
        });

        Swal.fire("Deleted!", "Shopping Cart Empty", "success");
      }
    });
  };

  const onPurchase = () => {
    Swal.fire("Purchase", "Purchase Completed", "success").then(async () => {
      // add CompleteShoppingCart request here
      await CartService.CompleteCartTransaction(token);
      setShoppingCart({
        items: [],
        countProducts: 0,
        total: 0,
      });
    });
  };

  const handleRemoveItem = async (itemId: number) => {
    await CartService.DeleteProductFromShoppingCart(itemId, token);
    const shoppingCart: ShoppingCart = await ShoppingCartUtils.getShoppingCart(
      token
    );
    setShoppingCart(shoppingCart);
  };

  const handleUpdateQuantity = async (productId: number, quantity: number) => {
    await CartService.addProductToShoppingCart(productId, quantity, token);
    const shoppingCart: ShoppingCart = await ShoppingCartUtils.getShoppingCart(
      token
    );
    setShoppingCart(shoppingCart);
  };

  return (
    <div className="container-icon">
      <div className="container-cart-icon" onClick={() => setActive(!active)}>
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          strokeWidth="1.5"
          stroke="currentColor"
          className="icon-cart"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M15.75 10.5V6a3.75 3.75 0 10-7.5 0v4.5m11.356-1.993l1.263 12c.07.665-.45 1.243-1.119 1.243H4.25a1.125 1.125 0 01-1.12-1.243l1.264-12A1.125 1.125 0 015.513 7.5h12.974c.576 0 1.059.435 1.119 1.007zM8.625 10.5a.375.375 0 11-.75 0 .375.375 0 01.75 0zm7.5 0a.375.375 0 11-.75 0 .375.375 0 01.75 0z"
          />
        </svg>
        <div className="count-products">
          <span id="contador-productos">{shoppingCart.countProducts}</span>
        </div>
      </div>

      <div className={`container-cart-products ${active ? "" : "hidden-cart"}`}>
        {shoppingCart.items.length ? (
          <>
            <div className="row-product">
              {shoppingCart.items.sort().map((item: Item) => (
                <div className="cart-product" key={item.id}>
                  <div className="info-cart-product">
                    <span className="titulo-producto-carrito">
                      {item.productReference.name}
                    </span>
                    <span className="precio-producto-carrito">
                      ${item.totalPrice}
                    </span>
                  </div>
                  <Counter
                    removeProductCallback={() => handleRemoveItem(item.id)}
                    productId={item.idProduct}
                    handleUpdateQuantity={handleUpdateQuantity}
                    quantity={item.quantity}
                  />
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    strokeWidth="1.5"
                    stroke="currentColor"
                    className="icon-close"
                    onClick={() => onDeleteProduct(item)}
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </div>
              ))}
            </div>

            <div className="cart-total">
              <h3>Total:</h3>
              <span className="total-pagar">${shoppingCart.total}</span>
            </div>

            <button className="btn-clear-all" onClick={onCleanCart}>
              Vaciar Carrito
            </button>

            <button className="btn-purchase" onClick={onPurchase}>
              Finalizar Compra
            </button>
          </>
        ) : (
          <p className="cart-empty">El carrito está vacío</p>
        )}
      </div>
    </div>
  );
};
