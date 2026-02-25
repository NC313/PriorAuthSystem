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

export default client;
