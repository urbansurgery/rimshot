import firebase from 'firebase/app';
import 'firebase/auth';
import 'firebase/firestore';
import 'firebase/functions';

import { config } from '../../../../configuration/firebaseConfig';

export const db = firebase.initializeApp(config).firestore();
