import { environment } from "../config/environment";
import { tokenStorage } from "./tokenStorage";

export class ApiError extends Error {
  public readonly status: number;

  public constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

type RequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
};

export class ApiClient {
  public async get<TResponse>(path: string): Promise<TResponse> {
    return this.request<TResponse>(path, { method: "GET" });
  }

  public async post<TResponse, TBody = unknown>(path: string, body?: TBody): Promise<TResponse> {
    return this.request<TResponse>(path, { method: "POST", body });
  }

  public async put<TResponse, TBody = unknown>(path: string, body?: TBody): Promise<TResponse> {
    return this.request<TResponse>(path, { method: "PUT", body });
  }

  public async delete(path: string): Promise<void> {
    await this.request<void>(path, { method: "DELETE" });
  }

  private async request<TResponse>(path: string, options: RequestOptions): Promise<TResponse> {
    const { body, ...requestOptions } = options;
    const headers = new Headers(requestOptions.headers);
    headers.set("Accept", "application/json");

    const token = tokenStorage.getAccessToken();
    if (token) {
      headers.set("Authorization", `Bearer ${token}`);
    }

    const init: RequestInit = {
      ...requestOptions,
      headers
    };

    if (body !== undefined) {
      headers.set("Content-Type", "application/json");
      init.body = JSON.stringify(body);
    }

    const response = await fetch(`${environment.apiBaseUrl}${path}`, init);
    if (!response.ok) {
      throw new ApiError(await readErrorMessage(response), response.status);
    }

    if (response.status === 204) {
      return undefined as TResponse;
    }

    return response.json() as Promise<TResponse>;
  }
}

async function readErrorMessage(response: Response): Promise<string> {
  try {
    const body = (await response.json()) as { error?: string };
    return body.error || "Request failed.";
  } catch {
    return "Request failed.";
  }
}

export const apiClient = new ApiClient();
