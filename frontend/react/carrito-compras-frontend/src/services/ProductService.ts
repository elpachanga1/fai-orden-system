
import axios from "axios";
import { Product } from "../entities/Interfaces";

const apiUrl: string = process.env.REACT_APP_API_URL || "";

export const getProducts = async (token: string): Promise<Product[]> => {
    try {
        const response = await axios.get<Product[]>(`${apiUrl}/Product/GetAllProducts`, 
        { headers: {Authorization: `Bearer ${token}`}});
        console.log(response);
        return response.data;
    } catch (error: any) {
        console.log("Error: ", error.message);
        return [];
    }
};