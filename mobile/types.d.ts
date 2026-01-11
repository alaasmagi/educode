import { NavigationProp, RouteProp } from '@react-navigation/native';
import { RootStackParamList } from './your-navigation-file'; // or wherever your stack params are defined

declare global {
  type NavigationProps<T extends keyof RootStackParamList = keyof RootStackParamList> = {
    navigation: NavigationProp<RootStackParamList, T>;
    route: RouteProp<RootStackParamList, T>;
  };
}
