import { useEffect, useState } from 'react';
import { Product, ShoppingCart } from '../entities/Interfaces';
import * as CartService from '../services/CartService';
import * as ProductService from '../services/ProductService';
import * as ShoppingCartUtils from '../utils/ShoppingCartUtils';
import { getToken } from '../utils/tokenUtil';
import { sortBy } from 'lodash';

export const ProductList = ({
	setShoppingCart,
}: any) => {
	const [products, setProducts] = useState<Product[]>([]);
	const [token, setToken] = useState<string>('');
	
	useEffect(() => {
		const rawToken = getToken()
		setToken(rawToken);
		const fetchProducts = async () => {
			try {
				const products: Product[] = await ProductService.getProducts(rawToken);
				setProducts(products);
			} catch (error) {
				console.error('Error fetching products:', error);
			}
		};
	
		fetchProducts();
	}, []);
	
	
	  
	const onAddProduct = async (product: Product) => {
		await CartService.addProductToShoppingCart(product.id, 1, token);
		const shoppingCart: ShoppingCart = await ShoppingCartUtils.getShoppingCart(token);
		setShoppingCart(shoppingCart);
	};

	return (
		<div className='container-items'>
			{sortBy(products, 'sku').map(product => (
				<div className='item' key={product.id}>
					<figure>
						<img src={product.image} alt={product.name} />
					</figure>
					<div className='info-product'>
						<h2>{product.name}</h2>
						<p className='unitPrice'>${product.unitPrice}</p>
						<button onClick={() => onAddProduct(product)}>
							Añadir al carrito
						</button>
					</div>
				</div>
			))}
		</div>
	);
};
