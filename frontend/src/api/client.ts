import axios from 'axios';

const client = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
});

client.interceptors.request.use((config) => {
  const role = localStorage.getItem('demo_role');
  if (role) {
    config.headers['X-Demo-Role'] = role;
  }
  return config;
});

client.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status;
    const detail = error?.response?.data?.detail ?? error?.response?.data?.title ?? error?.response?.data;
    if (status === 403) {
      return Promise.reject(new Error('You do not have permission to perform this action.'));
    }
    if (status === 404) {
      return Promise.reject(new Error('The requested resource was not found.'));
    }
    if (status === 400 && typeof detail === 'string') {
      return Promise.reject(new Error(detail));
    }
    return Promise.reject(error);
  }
);

export default client;
