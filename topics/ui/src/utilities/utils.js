export const sortByKey = (array, keyToSort, direction) => {
  if (direction === 'none') return array;

  function compare(A, B) {
    const a = A[keyToSort];
    const b = B[keyToSort];

    if (a === b) return 0;

    if (a > b) {
      return direction === 'asc' ? 1 : -1;
    } else {
      return direction === 'asc' ? -1 : 1;
    }
  }

  if (array) {
    return array.slice().sort(compare);
  }
  return array;
};
