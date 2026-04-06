import { FunctionComponent, useState } from "react";

const svgUrl = (name: string) => `${process.env.PUBLIC_URL}/svg/${name}.svg`;

interface Props {
  removeProductCallback: (productId: number) => void;
  handleUpdateQuantity: (productId: number, quantity: number) => void;
  productId: number;
  quantity: number;
}
export const Counter: FunctionComponent<Props> = ({
  removeProductCallback,
  handleUpdateQuantity,
  productId,
  quantity
}) => {
  const [value, setValue] = useState<number>(quantity);

  const reduce = (): void => {
    setValue((prevState) => {
      const updatedValue = prevState - 1;
      if (updatedValue === 0) {
        removeProductCallback(productId);
      } else {
        handleUpdateQuantity(productId, updatedValue);
      }
      return updatedValue;
    });
  };

  const increase = (): void => {
    setValue((prevState) => {
      const updateValue = prevState + 1;
      handleUpdateQuantity(productId, updateValue);
      return updateValue;
    });
  };
  // https://www.svgrepo.com/svg/521942/add-ellipse
  return (
    <div className="counter-container">
      {value === 1 ? (
        <img
          className="counter-button"
          src={svgUrl("trash")}
          alt="Remove item"
          onClick={reduce}
        />
      ) : (
        <img
          className="counter-button"
          src={svgUrl("remove-circle")}
          alt="Decrease quantity"
          onClick={reduce}
        />
      )}

      <span className="counter-label">{value}</span>
      <img
        className="counter-button"
        src={svgUrl("add-circle")}
        alt="Increase quantity"
        onClick={increase}
      />
    </div>
  );
};
