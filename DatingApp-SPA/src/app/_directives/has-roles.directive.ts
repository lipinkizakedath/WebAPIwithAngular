import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Directive({
  selector: '[appHasRoles]'
})
export class HasRolesDirective implements OnInit {
  @Input() appHasRoles: string[];
  isVisible = false;

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private authSerive: AuthService) { }


  ngOnInit() {
    const userRoles = this.authSerive.decodedToken.role as Array<string>;

    // if user has no roles then do this
    if (!userRoles) {
      this.viewContainerRef.clear();
    }

    // if user has some roles then do this

    if (this.authSerive.roleMatch(this.appHasRoles)) {
      if (!this.isVisible) {
        this.isVisible = true;
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      } else {
        this.isVisible = false;
        this.viewContainerRef.clear();
      }
    }

  }
}
