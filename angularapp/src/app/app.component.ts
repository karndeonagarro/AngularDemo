import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { environment } from './../environments/environment';
import { Product } from './Models/product';
import { ProductService } from './Services/product.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  products: Product[] = [];
  pageNumber = 1;
  pageSize = 4;
  sortField = 'Name';
  sortOrder = 'ASC';

 

  constructor(private productService: ProductService) {
  }

  title = 'angularapp';
  ngOnInit(): void {
    //this.fetchDataWithOracle();
    this.fetchDataWithMSSql();
  }

  fetchDataWithMSSql(): void {
    this.productService.getProductsWithMSSql()
      .subscribe(data => {
        this.products = data;
      });
  }

  fetchDataWithOracle(): void {
    this.productService.getProducts(this.pageNumber, this.pageSize, this.sortField, this.sortOrder)
      .subscribe(data => {
        this.products = data;
      });
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.fetchDataWithOracle();
  }

  onSort(field: string): void {
    if (field === this.sortField) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortOrder = 'asc';
    }
    this.fetchDataWithOracle();
  }
}

