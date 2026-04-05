
import axios from "axios";

const apiUrl: string = process.env.REACT_APP_API_URL || "";

export const addProductToShoppingCart = async (productId: number, quantity: number, token: string): Promise<any> => {
    try {
        const response = await axios.post<any>(`${apiUrl}/Store/AddProductToShoppingCart?IdUser=${1}&IdProduct=${productId}&Quantity=${quantity}`,{},
        { headers: {Authorization: `Bearer ${token}`}});
        console.log(response);
        return response.data;
    } catch (error: any) {
        console.log("Error: ", error.message);
        return [];
    }
};

export const EmptyShoppingCart = async (token: string): Promise<any> => {
    try {
        const response = await axios.delete<any>(`${apiUrl}/Store/EmptyShoppingCart?IdUser=${1}`,
        { headers: {Authorization: `Bearer ${token}`}});
        console.log(response);
        return response.data;
    } catch (error: any) {
        console.log("Error: ", error.message);
        return [];
    }
};

export const DeleteProductFromShoppingCart = async (idItem: number, token: string): Promise<any> => {
    try {
        const response = await axios.delete<any>(`${apiUrl}/Store/DeleteProductFromShoppingCart?IdUser=${1}&IdItem=${idItem}`,
        { headers: {Authorization: `Bearer ${token}`}});
        console.log(response);
        return response.data;
    } catch (error: any) {
        console.log("Error: ", error.message);
        return [];
    }
};
export const UpdateProductFromShoppingCart = async (IdProduct: number, quantity: number, token: string): Promise<any> => {
    try {
        const response = await axios.post<any>(`${apiUrl}/Store/AddProductToShoppingCart?IdUser=${1}&IdProduct=${IdProduct}&Quantity=${quantity}`,{},
        { headers: {Authorization: `Bearer ${token}`}});
        console.log(response);
        return response.data;
    } catch (error: any) {
        console.log("Error: ", error.message);
        return [];
    }

};
export const CompleteCartTransaction = async (token: string): Promise<any> => {
    try {
        const response = await axios.post<any>(`${apiUrl}/Store/CompleteCarTransaction?IdUser=${1}}`, {},
        { headers: {Authorization: `Bearer ${token}`}});
        console.log(response);
        return response.data;
    } catch (error: any) {
        console.log("Error: ", error.message);
        return [];
    }
}
