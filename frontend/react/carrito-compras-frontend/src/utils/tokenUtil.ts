import {get} from 'lodash';
export const getToken = ()=> {
    const userLogged = JSON.parse(localStorage.getItem('userLogged') || '{}');
	const rawToken = get(userLogged, 'sessionReference.token', '');
    return rawToken;
}