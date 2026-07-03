const requiredApiBaseUrl = import.meta.env.VITE_API_BASE_URL;

if (!requiredApiBaseUrl) {
  throw new Error("VITE_API_BASE_URL is not configured.");
}

export const environment = {
  apiBaseUrl: requiredApiBaseUrl.replace(/\/$/, "")
};
