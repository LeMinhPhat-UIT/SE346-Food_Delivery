type DecimalLike = {
  toNumber: () => number;
} | number | null | undefined;

export const toNumber = (value: DecimalLike) => {
  if (value === null || typeof value === "undefined") {
    return 0;
  }

  return typeof value === "number" ? value : value.toNumber();
};
