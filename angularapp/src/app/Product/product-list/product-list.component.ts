import { Component, OnInit } from '@angular/core';
import { Product } from '../../Models/product';
import { ProductService } from '../../Services/product.service';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  cols: any[] = [];
  pageNumber = 1;
  pageSize = 4;
  sortField = 'Name';
  sortOrder = 'ASC';


  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    this.fetchData();
    this.cols = [
      { field: 'name', header: 'Name' },
      { field: 'description', header: 'Description' },
      { field: 'category', header: 'Category' },
      { field: 'price', header: 'Price' },
    ];
  }

  fetchData(): void {
    this.productService.getProducts(this.pageNumber, this.pageSize, this.sortField, this.sortOrder)
      .subscribe(data => {
        this.products = data;
      });
  }

}

