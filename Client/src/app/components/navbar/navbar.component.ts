import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../providers/auth.service';
import { TokenService } from '../../providers/token.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit, OnDestroy {
  constructor(
    private authService: AuthService,
    private tokenService: TokenService
  ) { }

  isExpanded = false;
  isAuthentiacated: boolean = false;

  ngOnInit() {
    this.authService.authListener.subscribe(authorized => {
      this.isAuthentiacated = authorized;
    });
    this.isAuthentiacated = this.tokenService.IsAuthenticated;
  }

  ngOnDestroy() {
    this.authService.authListener.unsubscribe();
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  logout() {
    this.authService.logout();
  }
}
