import { Component, OnInit, TemplateRef } from '@angular/core';
import { Router } from '@angular/router';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CreateProductStockCommand } from '../../../models/CreateProductStockCommand';
import { ProductStockDetailsDTO } from '../../../models/ProductStockDetailsDTO';
import { ProductStockDTO } from '../../../models/ProductStockDTO';
import { UnitTypeDTO } from '../../../models/UnitTypeDTO';
import { UpdateProductStockCommand } from '../../../models/UpdateProductStockCommand';
import { UpdateProductStockDetailsCommand } from '../../../models/UpdateProductStockDetailsCommand';
import { ProductStocksService } from '../../../providers/product-stocks.service';
//ProductStockClient,
//ProductStockDTO,
//ProductStockDetailsVm,
//CreateProductStockCommand,
//UpdateProductStockCommand,
//UpdateProductStockDetailsCommand,
//UnitTypeDTO

@Component({
  selector: 'app-product-stocks',
  templateUrl: './product-stocks.component.html'
})
export class ProductStocksComponent implements OnInit {
  public productStocks: ProductStockDTO[];
  unitTypes: UnitTypeDTO[];
  debug = false;
  selectedProductStockUnits: ProductStockDTO | null;
  productStockEditor: any = {};
  newProductStockEditor: any = {};
  productStockModalRef: BsModalRef;
  newProductStockModalRef: BsModalRef;
  deleteProductStockModalRef: BsModalRef;

  constructor(
    private productStockService: ProductStocksService,
    private modalService: BsModalService,
    private router: Router
  ) {
    this.router.routeReuseStrategy.shouldReuseRoute = () => {
      return false;
    };
  }

  ngOnInit(): void {
    this.productStockService.getAll().subscribe(
      result => {
        this.productStocks = result.productStocks;
        this.unitTypes = result.unitTypes;
      },
      error => console.error(error)
    );
  }

  showNewProductStockModal(template: TemplateRef<any>): void {
    this.newProductStockModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('name')?.focus(), 250);
  }

  newProductStockCancelled(): void {
    this.newProductStockModalRef.hide();
    this.newProductStockEditor = {};
  }

  getUnitTypeNameFromUnitTypeValue(unitTypeValue: number | undefined): string | undefined {
    for (var unitType of this.unitTypes) {
      if (unitType.value == unitTypeValue) {
        return unitType.name;
      }
    }
    return "Unknown";
  }

  addProductStock(): void {
    const productStock = {
      id: 0,
      name: this.newProductStockEditor.name,
      productSearchItems: []
    } as ProductStockDetailsDTO;

    this.productStockService.create(productStock as CreateProductStockCommand).subscribe(
      result => {
        productStock.id = result;
        this.productStocks.push(productStock);
        this.newProductStockModalRef.hide();
        this.newProductStockEditor = {};
      },
      error => {
        const errors = JSON.parse(error.response);

        if (errors && errors.Title) {
          this.newProductStockEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('userImport')?.focus(), 250);
      }
    );
  }

  editProductStockUnits(productStock: ProductStockDTO, inputId: string): void {
    this.selectedProductStockUnits = productStock;
    setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  }

  updateProductStockUnits(productStock: ProductStockDTO, pressedEnter: boolean = false): void {
    const updateProductStockCommand = productStock as UpdateProductStockCommand;
    this.productStockService.update(productStock.id, updateProductStockCommand).subscribe(
      result => {
        for (var i = this.productStocks.length - 1; i >= 0; i--) {
          if (this.productStocks[i].id == result.id) {
            this.productStocks[i] = result;
            break;
          }
        }
        this.selectedProductStockUnits = null;
      },
      error => console.error(error)
    );
  }

  updateProductStockDetails(): void {
    const updateProductStockDetailsCommand = this._cast<UpdateProductStockDetailsCommand>(this.productStockEditor);
    this.productStockService.updateProductStockDetails(this.productStockEditor.id, updateProductStockDetailsCommand).subscribe(
      result => {
        for (var i = this.productStocks.length - 1; i >= 0; i--) {
          if (this.productStocks[i].id == result.id) {
            this.productStocks[i] = result;
            break;
          }
        }
        if (this.productStockEditor.id != result.id) {
          //product stock was merged into another product stock. remove the old one
          for (var i = this.productStocks.length - 1; i >= 0; i--) {
            if (this.productStocks[i].id == this.productStockEditor.id) {
              this.productStocks.splice(i, 1);
              break;
            }
          }
        }
        this.productStockModalRef.hide();
        this.productStockEditor = {};
      },
      error => console.error(error)
    );
  }

  _cast<T extends Object>(object: any): T {
    let returnValue: T = { ...object } as T;
    Object.keys(object).forEach(key => {
      const value = (returnValue as any)[key];
      (returnValue as any)[key] = value;
    });
    return returnValue;
  }

  showProductStockDetailsModal(template: TemplateRef<any>, productStock: ProductStockDTO): void {
    this.productStockService.getProductStockDetails(productStock.id, productStock.name).subscribe(
      result => {
        for (var i = this.productStocks.length - 1; i >= 0; i--) {
          if (this.productStocks[i].id == result.id) {
            this.productStocks[i] = result;
            break;
          }
        }

        this.productStockEditor = {
          ...result,
          search: result.name
        };

        this.productStockModalRef = this.modalService.show(template);
      },
      error => console.error(error)
    );
  }

  searchProductName(): void {
    this.productStockService.getProductStockDetails(this.productStockEditor.id, this.productStockEditor.search).subscribe(
      result => {
        for (var i = this.productStocks.length - 1; i >= 0; i--) {
          if (this.productStocks[i].id == result.id) {
            this.productStocks[i] = result;
            break;
          }
        }
        var oldSearch = this.productStockEditor.search;
        this.productStockEditor = {
          ...result,
          search: oldSearch
        };
      },
      error => {
        const errors = JSON.parse(error.response);

        if (errors && errors.Title) {
          this.productStockEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('search')?.focus(), 250);
      }
    );
  }

  confirmDeleteProductStock(template: TemplateRef<any>) {
    this.productStockModalRef.hide();
    this.deleteProductStockModalRef = this.modalService.show(template);
  }

  deleteProductStockConfirmed(): void {
    this.productStockService.delete(this.productStockEditor.id).subscribe(
      () => {
        this.deleteProductStockModalRef.hide();
        this.productStocks = this.productStocks.filter(t => t.id !== this.productStockEditor.id);
      },
      error => console.error(error)
    );
  }

}
