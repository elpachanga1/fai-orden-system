
import axios from "axios";
import { User } from "../entities/Interfaces";

const apiUrl: string = process.env.REACT_APP_API_URL || "";

export const AuthenticateUser = async (username: string, password: string): Promise<User | undefined> => {
    try {
        const response = await axios.post<User | undefined>(`${apiUrl}/User/AuthenticateUser?username=${username}&password=${password}`);
        console.log(response);
        return response.data;
    } catch (error: any) {
        console.log("Error: ", error.message);
        return undefined;
    }
};