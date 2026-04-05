import { useEffect, useState } from "react";
import { Link, Outlet } from "react-router-dom";
import { getToken } from "../utils/tokenUtil";
import { isEmpty } from "lodash";
import { useNavigate, useLocation } from "react-router";
import { NavDropdown } from "react-bootstrap";

export const Layout = () => {
  const [token, setToken] = useState<string>("");
  const [showPopper, setShowPopper] = useState<boolean>(false);
  let location = useLocation();
  const navigate = useNavigate();
  useEffect(() => {
    const rawToken = getToken();
    setToken(rawToken);
  }, [location]);
  const logout = () => {
    localStorage.removeItem("userLogged");
    navigate("/login");
  };
  return (
    <div>
      {/* <ul>
            <li>
              <Link to="/">Public Page</Link>
            </li>
            <li>
              <Link to="/protected">Protected Page</Link>
            </li>
          </ul>     */}
      {!isEmpty(token) && (
        <header className="p-3 mb-3 border-bottom">
          <div className="container">
            <div className="d-flex flex-wrap align-items-center justify-content-end">
              <div className="dropdown text-end d-flex">
                <span
                  className="d-block link-body-emphasis text-decoration-none "
                  onClick={() => setShowPopper(!showPopper)}
                >
                  <img
                    src="https://github.com/mdo.png"
                    alt="mdo"
                    width="32"
                    height="32"
                    className="rounded-circle"
                  />
                </span>
                <NavDropdown
                  id="nav-dropdown-dark-example"
                  title=""
                  className="ml-2"
                  show={showPopper}
                  onToggle={() => setShowPopper(!showPopper)}
                >
                  <NavDropdown.Item onClick={logout}>Logout</NavDropdown.Item>
                </NavDropdown>
              </div>
            </div>
          </div>
        </header>
      )}

      <Outlet />
    </div>
  );
};
