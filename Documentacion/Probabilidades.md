# Probabilidades

## Pareto Escalonado

El proyecto usa una regla Pareto recurrente para rarezas, crecimiento y cantidades especiales.

Regla:

- 80% de caer en el resultado actual.
- Del 20% restante, 80% cae en el siguiente.
- Se repite hasta llegar al ultimo resultado.

## Crecimiento de Stats

Rangos:

```text
F, E, D, C, B, A, S, SS
```

Aplicacion:

- F debe ser muy comun.
- SS debe ser extremadamente raro.
- SS puede estar roto porque casi nunca aparece.

## Perks Iniciales

Cantidad de perks a nivel 1:

- 80%: 1 perk.
- 16%: 2 perks.
- 4%: 3 perks.

La rareza de cada perk usa tambien Pareto:

```text
Common, Rare, VeryRare, Epic, Legendary
```

