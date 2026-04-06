import axios from "axios";
import { Item } from "../entities/Interfaces";

const apiUrl: string = process.env.REACT_APP_API_URL || "";

export const getItemsByProductId = async (productId: number, token: string): Promise<Item[]> => {
    try {
        const response = await axios.get<Item[]>(`${apiUrl}/Product/GetItemsByProductId/${productId}`,
        { headers: {Authorization: `Bearer ${token}`}});
        console.log(response);
        return response.data;
    } catch (error: any) {
        console.log("Error: ", error.message);
        return [];
    }
};

export const getItems = async (token: string): Promise<Item[]> => {
    try {
        
        const response = await axios.get<Item[]>(`${apiUrl}/Item/GetAllItems`,
        { headers: {Authorization: `Bearer ${token}`}});
        console.log(response);
        return response.data;
    } catch (error: any) {
        console.log("Error: ", error.message);
        return [];
    }
};