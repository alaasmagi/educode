import { useRootNavigationState, Redirect } from "expo-router";
import { useEffect, useState } from "react";

export default function Index() {
    const rootNavigation = useRootNavigationState();
    const [isReady, setIsReady] = useState(false);

    useEffect(() => {
        if (rootNavigation?.key) {
            setIsReady(true);
        }
    }, [rootNavigation]);

    if (!isReady) {
        return null;
    }

    return <Redirect href="/initial-selection" />;
}