import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Product } from '../Models/product';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  public products?: Product[];
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getProducts(pageNumber: number, pageSize: number, sortField: string, sortOrder: string): Observable<Product[]> {
    const url = `${this.baseUrl}/products?pageNumber=${pageNumber}&pageSize=${pageSize}&sortField=${sortField}&sortOrder=${sortOrder}`;
    return this.http.get<Product[]>(url);
  }

  getProductsWithMSSql(): Observable<Product[]> {
    const url = `${this.baseUrl}/poductswithmssql`;
    return this.http.get<Product[]>(url);
  }
}
