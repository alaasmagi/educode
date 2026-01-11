import {useRootNavigationState, useRouter} from "expo-router";
import {useEffect} from "react";


export default function Index() {
    const router = useRouter();
    const rootNavigation = useRootNavigationState();

    useEffect(() => {
        const timeout = setTimeout(() => router.replace('/tabs/login'))
    }, []);
}