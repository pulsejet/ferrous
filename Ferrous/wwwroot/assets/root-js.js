const applicationServerPublicKey = 'BNGO41K1-_hTx0pYKwLgGg_goog8MBQ9BCXQOKnopeYxP8fNHaKkcEmJ6Edq25a3xGA95o0SNHZda_eQWZnyBVQ';

if ('serviceWorker' in navigator && 'PushManager' in window) {
  console.log('Service Worker and Push is supported');

  navigator.serviceWorker.register('/sw.js')
  .then(function(swReg) {
    console.log('Service Worker is registered', swReg);

    swRegistration = swReg;
  })
  .catch(function(error) {
    console.error('Service Worker Error', error);
  });
} else {
  console.warn('Push messaging is not supported');
  pushButton.textContent = 'Push Not Supported';
}
