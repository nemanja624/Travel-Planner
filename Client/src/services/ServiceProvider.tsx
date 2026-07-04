import {
  createContext,
  PropsWithChildren,
  useContext
} from "react";
import { adminUserService } from "./adminUserService";
import { authService } from "./authService";
import { sharingService } from "./sharingService";
import { tokenStorage } from "./tokenStorage";
import { tripService } from "./tripService";

export interface AppServices {
  adminUserService: typeof adminUserService;
  authService: typeof authService;
  sharingService: typeof sharingService;
  tokenStorage: typeof tokenStorage;
  tripService: typeof tripService;
}

const defaultServices: AppServices = {
  adminUserService,
  authService,
  sharingService,
  tokenStorage,
  tripService
};

const ServicesContext = createContext<AppServices | null>(null);

interface ServiceProviderProps extends PropsWithChildren {
  services?: AppServices;
}

export function ServiceProvider({ children, services = defaultServices }: ServiceProviderProps) {
  return <ServicesContext.Provider value={services}>{children}</ServicesContext.Provider>;
}

export function useServices() {
  const services = useContext(ServicesContext);
  if (!services) {
    throw new Error("useServices must be used inside ServiceProvider.");
  }

  return services;
}
