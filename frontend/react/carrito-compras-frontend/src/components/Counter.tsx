import { FunctionComponent, useState } from "react";
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
      }else{

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
          src={process.env.PUBLIC_URL + "/svg/trash.svg"}
          onClick={reduce}
        />
      ) : (
        <img
          className="counter-button"
          src={process.env.PUBLIC_URL + "/svg/remove-circle.svg"}
          onClick={reduce}
        />
      )}

      <span className="counter-label">{value}</span>
      <img
        className="counter-button"
        src={process.env.PUBLIC_URL + "/svg/add-circle.svg"}
        onClick={increase}
      />
    </div>
  );
};
