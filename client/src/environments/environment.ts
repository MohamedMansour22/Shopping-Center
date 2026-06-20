export const environment = {
  production: false,
  apiUrl: 'http://localhost:5256/api',

  // Firebase Cloud Messaging (admin push notifications).
  // Fill these from the Firebase console → Project settings → General → "Your apps" (web app config),
  // and the Web Push "vapidKey" from Cloud Messaging → Web configuration → Web Push certificates.
  // Leave the values empty to disable push notifications (the rest of the app works unchanged).
  // NOTE: the same web-app config must also be mirrored in public/firebase-messaging-sw.js.
  firebase: {
    apiKey: '',
    authDomain: '',
    projectId: '',
    messagingSenderId: '',
    appId: '',
  },
  // Web Push certificate public key ("vapidKey").
  firebaseVapidKey: '',
};
