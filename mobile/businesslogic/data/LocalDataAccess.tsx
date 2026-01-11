import * as SecureStore from 'expo-secure-store';

const Storage = {
  async saveData(key: string, value: any) {
    try {
      const jsonValue = JSON.stringify(value);
      await SecureStore.setItemAsync(key, jsonValue);
    } catch (error) {
      console.error("Error saving data:", error);
    }
  },

  async getData<T>(key: string): Promise<T | null> {
    try {
      const jsonValue = await SecureStore.getItemAsync(key);
      return jsonValue ? JSON.parse(jsonValue) : null;
    } catch (error) {
      console.error("Error retrieving data:", error);
      return null;
    }
  },

  async removeData(key: string) {
    try {
      await SecureStore.deleteItemAsync(key);
    } catch (error) {
      console.error("Error removing data:", error);
    }
  },
};

export default Storage;
