import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CreateProductCommand } from '../../../models/CreateProductCommand';
import { ProductDTO } from '../../../models/ProductDTO';
import { UnitTypeDTO } from '../../../models/UnitTypeDTO';
import { UpdateProductCommand } from '../../../models/UpdateProductCommand';
import { UpdateProductNameCommand } from '../../../models/UpdateProductNameCommand';
import { UpdateProductSizeCommand } from '../../../models/UpdateProductSizeCommand';
import { UpdateProductUnitTypeCommand } from '../../../models/UpdateProductUnitTypeCommand';
import { ProductsService } from '../../../providers/products.service';
//UpdateProductCommand,
//UnitTypeDTO

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
  debug: boolean = false;
  products?: ProductDTO[];
  unitTypes?: UnitTypeDTO[];
  selectedProductName?: ProductDTO;
  selectedProductSize?: ProductDTO;
  selectedProductUnitType?: ProductDTO;
  productEditor: any = {};
  newProductEditor: any = {};
  productModalRef: BsModalRef;
  newProductModalRef: BsModalRef;
  deleteProductModalRef: BsModalRef;

  constructor(
    private productsService: ProductsService,
    private modalService: BsModalService,
    private router: Router
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => {
      return false;
    };
  }


  ngOnInit(): void {
    this.productsService.getAll().subscribe(
      result => {
        this.products = result.products;
        this.unitTypes = result.unitTypes;
      },
      error => console.error(error)
    );
  }


  showNewProductModal(template: TemplateRef<any>): void {
    this.newProductModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('name')?.focus(), 250);
  }

  newProductCancelled(): void {
    this.newProductModalRef.hide();
    this.newProductEditor = {};
  }

  getUnitTypeNameFromUnitTypeValue(unitTypeValue: number | undefined): string {
    if (this.unitTypes) {
      for (var unitType of this.unitTypes) {
        if (unitType.value == unitTypeValue) {
          return unitType.name || "Unknown";
        }
      }
    }
    return "Unknown";
  }

  addProduct(): void {
    const product = {
      id: 0,
      name: this.newProductEditor.name
    } as ProductDTO;

    this.productsService.create(product as CreateProductCommand).subscribe(
      result => {
        product.id = result;
        this.products?.push(product);
        this.newProductModalRef.hide();
        this.newProductEditor = {};
      },
      error => {
        const errors = JSON.parse(error.response);

        if (errors && errors.Title) {
          this.newProductEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('name')?.focus(), 250);
      }
    );
  }

  editProductName(product: ProductDTO, inputId: string): void {
    this.selectedProductName = product;
    setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  }

  editProductUnitType(product: ProductDTO, inputId: string): void {
    this.selectedProductUnitType = product;
    setTimeout(() => {
      document.getElementById(inputId)?.focus();
      (<HTMLSelectElement>document.getElementById(inputId)).size = (<HTMLSelectElement>document.getElementById(inputId)).length;
    }, 100);
  }

  editProductSize(product: ProductDTO, inputId: string): void {
    this.selectedProductSize = product;
    setTimeout(() => { document.getElementById(inputId)?.focus(); }, 100);
  }

  updateProductName(product: ProductDTO, pressedEnter: boolean = false): void {
    const updateProductNameCommand = this._cast(product, UpdateProductNameCommand);
    this.productsService.updateName(product.id, updateProductNameCommand).subscribe(
      result => {
        if (this.products) {
          for (var i = this.products.length - 1; i >= 0; i--) {
            if (this.products[i].id == result.id) {
              this.products[i] = result;
              break;
            }
          }
          this.selectedProductName = undefined;
        }
      },
      error => console.error(error)
    );
  }

  updateProductUnitType(product: ProductDTO, pressedEnter: boolean = false): void {
    const updateProductUnitTypeCommand = this._cast(product, UpdateProductUnitTypeCommand);
    this.productsService.updateUnitType(product.id, updateProductUnitTypeCommand).subscribe(
      result => {
        if (this.products) {
          for (var i = this.products.length - 1; i >= 0; i--) {
            if (this.products[i].id == result.id) {
              this.products[i] = result;
              break;
            }
          }
          this.selectedProductUnitType = undefined;
        }
      },
      error => console.error(error)
    );
  }

  updateProductSize(product: ProductDTO, pressedEnter: boolean = false): void {
    const updateProductSizeCommand = product as UpdateProductSizeCommand;
    this.productsService.updateSize(product.id, updateProductSizeCommand).subscribe(
      result => {
        if (this.products) {
          for (var i = this.products.length - 1; i >= 0; i--) {
            if (this.products[i].id == result.id) {
              this.products[i] = result;
              break;
            }
          }
          this.selectedProductSize = undefined;
        }
      },
      error => console.error(error)
    );
  }

  searchProductName(): void {
    const updateProductNameCommand = this.productEditor as UpdateProductCommand;
    this.productsService.updateName(this.productEditor.id, updateProductNameCommand).subscribe(
      result => {
        this.productEditor = result;
        if (this.products) {
          for (var i = this.products.length - 1; i >= 0; i--) {
            if (this.products[i].id == this.productEditor.id) {
              this.products[i] = this.productEditor;
              break;
            }
          }
          if (this.productEditor.walmartSearchResponse) {
            this.productEditor.walmartSearchItems = JSON.parse(this.productEditor.walmartSearchResponse).items;
          }
        }
      },
      error => console.error(error)
    );
  }

  updateProductDetails(): void {
    const updateProductCommand = this.productEditor as UpdateProductCommand;
    this.productsService.update(this.productEditor.id, updateProductCommand).subscribe(
      result => {
        this.productEditor = result;
        if (this.products) {
          for (var i = this.products.length - 1; i >= 0; i--) {
            if (this.products[i].id == this.productEditor.id) {
              this.products[i] = this.productEditor;
              break;
            }
          }
          if (this.productEditor.walmartSearchResponse) {
            this.productEditor.walmartSearchItems = JSON.parse(this.productEditor.walmartSearchResponse).items;
          }
          this.productModalRef.hide();
          this.productEditor = null;
        }
      },
      error => console.error(error)
    );
  }

  showProductDetailsModal(template: TemplateRef<any>, product: ProductDTO): void {
    this.productsService.getProductDetails(product.id).subscribe(
      result => {
        if (this.products) {
          for (var i = this.products.length - 1; i >= 0; i--) {
            if (this.products[i].id == result.id) {
              this.products[i] = result;
              break;
            }
          }

          this.productEditor = {
            ...result
          };
          if (this.productEditor.walmartSearchResponse) {
            this.productEditor.walmartSearchItems = JSON.parse(this.productEditor.walmartSearchResponse).items;
          }

          this.productModalRef = this.modalService.show(template);
        }
      },
      error => console.error(error)
    );
  }

  deleteProduct(product: ProductDTO | undefined) {
    if (product) {
      if (this.productModalRef) {
        this.productModalRef.hide();
      }
      if (this.products) {
        if (product.id === 0) {
          if (this.selectedProductName) {
            const completedOrderProductIndex = this.products.indexOf(this.selectedProductName);
            this.products.splice(completedOrderProductIndex, 1);
          }
        } else {
          this.productsService.delete(product.id).subscribe(
            () =>
            (this.products = this.products?.filter(
              t => t.id !== product.id
            )),
            error => console.error(error)
          );
        }
      }
    }
  }

  _cast<K extends T, T>(obj: K, tClass: { new(...args: any[]): K }): K {
    let returnObject: K = new tClass();
    for (let p in returnObject) {
      const value = obj[p] || undefined;
      if (value != undefined) {
        returnObject[p] = value;
      }
    }
    return returnObject;
  }


}
