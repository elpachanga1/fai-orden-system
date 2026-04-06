export function getToken(): string {
  try {
    const raw = localStorage.getItem('userLogged');
    if (!raw) return '';
    const userLogged = JSON.parse(raw);
    return userLogged?.sessionReference?.token ?? '';
  } catch {
    return '';
  }
}
