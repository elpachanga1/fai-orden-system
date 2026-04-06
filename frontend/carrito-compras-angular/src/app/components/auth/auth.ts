import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-auth',
  imports: [FormsModule],
  templateUrl: './auth.html',
  styleUrl: './auth.css'
})
export class AuthComponent {
  username = '';
  password = '';

  constructor(private userService: UserService, private router: Router) {}

  async onLogin(): Promise<void> {
    if (!this.username || !this.password) {
      Swal.fire('Oops...', 'Username and Password Should not be Void', 'warning');
      return;
    }

    const user = await this.userService.authenticateUser(this.username, this.password);
    if (user) {
      localStorage.setItem('userLogged', JSON.stringify(user));
      this.router.navigate(['/home']);
    } else {
      Swal.fire('Oops...', 'Invalid Credentials', 'warning');
    }
  }
}
