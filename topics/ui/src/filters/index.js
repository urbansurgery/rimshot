// import moment from 'moment';
import dayjs from 'dayjs';

export const isoDate = d => {
  if (d) {
    if (d.toDate) {
      d = d.toDate();
    }
    if (d.toISOString) {
      // return moment(d.toISOString()).format('YYYY-MM-DD');
      return dayjs(d.toISOString()).format('YYYY-MM-DD');
    }
  }
  return d;
};

export const trim = d => {
  if (d.length > 8) {
    return d.slice(-8).replace(/-/gi, '');
  }
  return d;
};

export const snakeCaseToTitleCase = str => {
  return str
    .toString()
    .replace(
      /^([a-z])|_([a-z])/gi,
      Attr.call.bind(String.prototype.toUpperCase)
    )
    .replace('_', ' ');
};

export const leftPad = (str, size, ch) => {
  str = str.toString();

  const padding = (size, ch) => {
    let str = '';
    if (!ch && ch !== 0) {
      ch = ' ';
    }
    while (size !== 0) {
      if (size & (1 === 1)) {
        str += ch;
      }
      ch += ch;
      size >>>= 1;
    }
    return str;
  };

  size = +size || 0;
  const padLength = size - str.length;
  if (padLength <= 0) {
    return str;
  }
  return padding(padLength, ch).concat(str);
};
