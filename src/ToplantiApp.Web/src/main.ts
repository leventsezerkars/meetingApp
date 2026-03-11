import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import moment from 'moment';

declare global {
  interface Window {
    moment: typeof moment;
  }
}
window.moment = moment;

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
