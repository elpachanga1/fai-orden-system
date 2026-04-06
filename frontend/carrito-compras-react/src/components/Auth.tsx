import Swal from "sweetalert2";
import * as UserService from "../services/UserService";
import { useNavigate } from "react-router";

export const AuthMenu = () => {
  const navigate = useNavigate();

  // Auth function, meter aqui la autenticacion y obtencion del token
  const getToken = async (e: any) => {
    e.preventDefault();
    const username = (
      document?.getElementById("userName") as HTMLInputElement | null
    )?.value;
    const password = (
      document?.getElementById("password") as HTMLInputElement | null
    )?.value;

    if (!username || !password) {
      Swal.fire(
        "Oops...",
        "Username and Password Should not be Void",
        "warning"
      );
      return;
    }

    const currentUser = await UserService.AuthenticateUser(username, password);
    if (currentUser) {
      localStorage.setItem("userLogged", JSON.stringify(currentUser));
      navigate("/home");
    } else {
      Swal.fire("Oops...", "Invalid Credentials", "warning");
    }
  };

  return (
    <div className="container col-xl-10  px-4 py-5">
      <div className="row align-items-center g-lg-5 py-5">
        <div className="col-lg-7 text-center text-lg-start">
          <h1 className="display-4 fw-bold lh-1 text-body-emphasis mb-3">
            Online ordering system
          </h1>
          <p className="col-lg-10 fs-4">
            It is necessary to log in to carry out verifications
          </p>
        </div>
        <div className="col-md-10 mx-auto col-lg-5">
          <form className="p-4 p-md-5 border rounded-3 bg-body-tertiary">
            <div className="form-floating mb-3">
              <input
                className="form-control"
                id="userName"
                placeholder="username"
              />
              <label htmlFor="userName">Username</label>
            </div>
            <div className="form-floating mb-3">
              <input
                type="password"
                className="form-control"
                id="password"
                placeholder="Password"
              />
              <label htmlFor="password">Password</label>
            </div>
            <div className="checkbox mb-3">
              <label>
                <input type="checkbox" value="remember-me" /> Remember me
              </label>
            </div>
            <button className="w-100 btn btn-lg btn-primary" onClick={getToken}>
              Sign up
            </button>
            <hr className="my-4" />
          </form>
        </div>
      </div>
    </div>
  );
};
