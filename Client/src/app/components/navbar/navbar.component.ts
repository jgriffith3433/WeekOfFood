//import { Component, OnInit } from '@angular/core';
//import { Router } from '@angular/router';
//import { TokenService } from 'src/app/providers/token.service';

//@Component({
//  selector: 'app-navbar',
//  templateUrl: './navbar.component.html',
//  styleUrls: ['./navbar.component.scss']
//})
//export class NavbarComponent implements OnInit {

//  constructor(public tokenService: TokenService, private router: Router) { }

//  ngOnInit(): void {
//  }

//  logout() {
//    this.tokenService.clearToken();
//    this.router.navigate(['/']);
//  }

//}

import { TokenService } from 'src/app/providers/token.service';
import { Router } from '@angular/router';
import { Component } from '@angular/core';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  constructor(public tokenService: TokenService, private router: Router) { }

  isExpanded = false;

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  logout() {
    this.tokenService.clearToken();
    this.router.navigate(['/']);
  }
}
