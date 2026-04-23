class PushNotificationService {
    constructor() {
        this.subscription = null;
        this.isSupported = 'serviceWorker' in navigator && 'PushManager' in window;
    }

    async init() {
        if (!this.isSupported) {
            console.log('[PushNotification] Push notifications not supported');
            return false;
        }

        try {
            // Register service worker
            const registration = await navigator.serviceWorker.register('/sw.js');
            console.log('[PushNotification] Service worker registered', registration);

            // Check existing subscription
            this.subscription = await registration.pushManager.getSubscription();
            
            if (this.subscription) {
                console.log('[PushNotification] Already subscribed', this.subscription);
                return true;
            }

            return false;
        } catch (error) {
            console.error('[PushNotification] Init error:', error);
            return false;
        }
    }

    async subscribe() {
        if (!this.isSupported) {
            throw new Error('Push notifications not supported');
        }

        try {
            // Register service worker
            const registration = await navigator.serviceWorker.register('/sw.js');
            
            // Check if already subscribed
            this.subscription = await registration.pushManager.getSubscription();
            
            if (this.subscription) {
                console.log('[PushNotification] Already subscribed, updating server');
                // Send existing subscription to server to ensure it's up to date
                await this.sendSubscriptionToServer(this.subscription);
                return true;
            }

            // Get VAPID public key
            const response = await fetch('/api/pushsubscription/vapid-public-key');
            const data = await response.json();
            
            if (!data.publicKey) {
                throw new Error('Failed to get VAPID public key');
            }

            // Convert base64 to Uint8Array
            const applicationServerKey = this.urlBase64ToUint8Array(data.publicKey);

            // Subscribe
            this.subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: applicationServerKey
            });

            console.log('[PushNotification] Subscribed', this.subscription);

            // Send subscription to server
            await this.sendSubscriptionToServer(this.subscription);

            return true;
        } catch (error) {
            console.error('[PushNotification] Subscribe error:', error);
            throw error;
        }
    }

    async unsubscribe() {
        if (!this.subscription) {
            console.log('[PushNotification] No subscription to unsubscribe');
            return false;
        }

        try {
            await this.subscription.unsubscribe();
            console.log('[PushNotification] Unsubscribed');

            // Remove from server
            await this.removeSubscriptionFromServer(this.subscription);

            this.subscription = null;
            return true;
        } catch (error) {
            console.error('[PushNotification] Unsubscribe error:', error);
            throw error;
        }
    }

    async sendSubscriptionToServer(subscription) {
        const subscriptionData = {
            endpoint: subscription.endpoint,
            keys: {
                p256dh: this.arrayBufferToBase64(subscription.getKey('p256dh')),
                auth: this.arrayBufferToBase64(subscription.getKey('auth'))
            }
        };

        console.log('[PushNotification] Sending subscription to server:', subscriptionData);

        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };

        const token = this.getAuthToken();
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch('/api/pushsubscription/subscribe', {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(subscriptionData),
            credentials: 'include'  // Include cookies for authentication
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error('[PushNotification] Server response:', response.status, errorText);
            throw new Error(`Failed to send subscription to server: ${response.status} - ${errorText}`);
        }

        console.log('[PushNotification] Subscription sent to server');
    }

    getAntiforgeryToken() {
        // Try to get from cookies
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === '.AspNetCore.Antiforgery' || name === 'X-CSRF-TOKEN') {
                return value;
            }
        }

        // Try to get from meta tag
        const metaTag = document.querySelector('meta[name="csrf-token"]');
        if (metaTag) {
            return metaTag.getAttribute('content');
        }

        // Try to get from input
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        if (input) {
            return input.value;
        }

        console.log('[PushNotification] No antiforgery token found');
        return null;
    }

    async removeSubscriptionFromServer(subscription) {
        const headers = {
            'Content-Type': 'application/json'
        };

        const token = this.getAuthToken();
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch('/api/pushsubscription/unsubscribe', {
            method: 'POST',
            headers: headers,
            body: JSON.stringify({ endpoint: subscription.endpoint }),
            credentials: 'include'  // Include cookies for authentication
        });

        if (!response.ok) {
            console.error('[PushNotification] Failed to remove subscription from server');
        }
    }

    arrayBufferToBase64(buffer) {
        let binary = '';
        const bytes = new Uint8Array(buffer);
        const len = bytes.byteLength;
        for (let i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    }

    getAuthToken() {
        // Get JWT token from localStorage, session storage, or cookie
        const token = localStorage.getItem('JWTToken') || sessionStorage.getItem('JWTToken');
        if (token) return token;

        // Try to get from cookie
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'JWTToken' || name === '.AspNetCore.Identity.Application') {
                return value;
            }
        }

        console.log('[PushNotification] No auth token found');
        return '';
    }

    urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/-/g, '+')
            .replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }

        return outputArray;
    }

    async getSubscription() {
        return this.subscription;
    }

    isSubscribed() {
        return this.subscription !== null;
    }
}

// Export for use in Blazor
window.PushNotificationService = new PushNotificationService();
