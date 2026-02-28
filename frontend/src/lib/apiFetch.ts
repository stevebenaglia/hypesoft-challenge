// Server-side: uses internal Docker network URL directly (INTERNAL_API_URL)
// Client-side: uses relative path so requests go through Nginx (/api/...)
const API_URL =
  typeof window === "undefined"
    ? (process.env.INTERNAL_API_URL ?? "http://backend:5000")
    : "";

interface FetchOptions extends RequestInit {
  accessToken?: string;
}

export async function apiFetch<T>(
  path: string,
  options: FetchOptions = {}
): Promise<T> {
  const { accessToken, headers, ...rest } = options;

  const response = await fetch(`${API_URL}${path}`, {
    ...rest,
    headers: {
      "Content-Type": "application/json",
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      ...headers,
    },
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ title: response.statusText }));
    throw new Error(error.title ?? `HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json();
}
