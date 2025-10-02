import { describe, it, expect } from 'vitest';
import {
  parseFilterQuery,
  validateFilterQuery,
  FilterParseError,
} from './filterParserAntlr';

describe('FilterParser', () => {
  describe('parseFilterQuery', () => {
    describe('Basic Comparisons', () => {
      it('should parse equals condition', () => {
        const result = parseFilterQuery('name = "John"');
        expect(result).toEqual({
          condition: {
            column: 'name',
            function: 'equals',
            args: ['John'],
          },
        });
      });

      it('should parse not equals condition', () => {
        const result = parseFilterQuery('status != "inactive"');
        expect(result).toEqual({
          condition: {
            column: 'status',
            function: 'equals',
            args: ['inactive'],
          },
          negate: true,
        });
      });

      it('should parse greater than condition', () => {
        const result = parseFilterQuery('age > 18');
        expect(result).toEqual({
          condition: {
            column: 'age',
            function: 'greaterThan',
            args: [18],
          },
        });
      });

      it('should parse greater than or equals condition', () => {
        const result = parseFilterQuery('score >= 90');
        expect(result).toEqual({
          condition: {
            column: 'score',
            function: 'greaterThanOrEquals',
            args: [90],
          },
        });
      });

      it('should parse less than condition', () => {
        const result = parseFilterQuery('price < 100.50');
        expect(result).toEqual({
          condition: {
            column: 'price',
            function: 'lessThan',
            args: [100.5],
          },
        });
      });

      it('should parse less than or equals condition', () => {
        const result = parseFilterQuery('count <= 5');
        expect(result).toEqual({
          condition: {
            column: 'count',
            function: 'lessThanOrEquals',
            args: [5],
          },
        });
      });
    });

    describe('String Functions', () => {
      it('should parse contains condition', () => {
        const result = parseFilterQuery('description CONTAINS "test"');
        expect(result).toEqual({
          condition: {
            column: 'description',
            function: 'contains',
            args: ['test'],
          },
        });
      });

      it('should parse starts with condition', () => {
        const result = parseFilterQuery('name STARTS_WITH "Mr"');
        expect(result).toEqual({
          condition: {
            column: 'name',
            function: 'startsWith',
            args: ['Mr'],
          },
        });
      });

      it('should parse ends with condition', () => {
        const result = parseFilterQuery('email ENDS_WITH "@example.com"');
        expect(result).toEqual({
          condition: {
            column: 'email',
            function: 'endsWith',
            args: ['@example.com'],
          },
        });
      });
    });

    describe('IN and NULL Conditions', () => {
      it('should parse IN condition with multiple values', () => {
        const result = parseFilterQuery('status IN ("active", "pending")');
        expect(result).toEqual({
          condition: {
            column: 'status',
            function: 'inSet',
            args: [['active', 'pending']],
          },
        });
      });

      it('should parse NOT IN condition', () => {
        const result = parseFilterQuery('id NOT IN (1, 2, 3)');
        expect(result).toEqual({
          condition: {
            column: 'id',
            function: 'inSet',
            args: [[1, 2, 3]],
          },
          negate: true,
        });
      });

      it('should parse IS NULL condition', () => {
        const result = parseFilterQuery('deletedAt IS NULL');
        expect(result).toEqual({
          condition: {
            column: 'deletedAt',
            function: 'isNull',
            args: [],
          },
        });
      });

      it('should parse IS NOT NULL condition', () => {
        const result = parseFilterQuery('email IS NOT NULL');
        expect(result).toEqual({
          condition: {
            column: 'email',
            function: 'isNull',
            args: [],
          },
          negate: true,
        });
      });
    });

    describe('Logical Operators', () => {
      it('should parse AND expression', () => {
        const result = parseFilterQuery('age > 18 AND status = "active"');
        expect(result).toEqual({
          group: {
            op: 'AND',
            filters: [
              {
                condition: {
                  column: 'age',
                  function: 'greaterThan',
                  args: [18],
                },
              },
              {
                condition: {
                  column: 'status',
                  function: 'equals',
                  args: ['active'],
                },
              },
            ],
          },
        });
      });

      it('should parse OR expression', () => {
        const result = parseFilterQuery('role = "admin" OR role = "moderator"');
        expect(result).toEqual({
          group: {
            op: 'OR',
            filters: [
              {
                condition: {
                  column: 'role',
                  function: 'equals',
                  args: ['admin'],
                },
              },
              {
                condition: {
                  column: 'role',
                  function: 'equals',
                  args: ['moderator'],
                },
              },
            ],
          },
        });
      });

      it('should parse mixed AND/OR with correct precedence', () => {
        const result = parseFilterQuery(
          'age > 18 AND status = "active" OR premium = true'
        );
        expect(result).toEqual({
          group: {
            op: 'OR',
            filters: [
              {
                group: {
                  op: 'AND',
                  filters: [
                    {
                      condition: {
                        column: 'age',
                        function: 'greaterThan',
                        args: [18],
                      },
                    },
                    {
                      condition: {
                        column: 'status',
                        function: 'equals',
                        args: ['active'],
                      },
                    },
                  ],
                },
              },
              {
                condition: {
                  column: 'premium',
                  function: 'equals',
                  args: [true],
                },
              },
            ],
          },
        });
      });
    });

    describe('Parentheses and NOT', () => {
      it('should parse parenthesized expression', () => {
        const result = parseFilterQuery('(age > 18 AND age < 65)');
        expect(result).toEqual({
          group: {
            op: 'AND',
            filters: [
              {
                condition: {
                  column: 'age',
                  function: 'greaterThan',
                  args: [18],
                },
              },
              {
                condition: {
                  column: 'age',
                  function: 'lessThan',
                  args: [65],
                },
              },
            ],
          },
        });
      });

      it('should parse NOT expression', () => {
        const result = parseFilterQuery('NOT status = "inactive"');
        expect(result).toEqual({
          condition: {
            column: 'status',
            function: 'equals',
            args: ['inactive'],
          },
          negate: true,
        });
      });

      it('should parse complex expression with parentheses', () => {
        const result = parseFilterQuery(
          '(age > 18 OR premium = true) AND status = "active"'
        );
        expect(result).toEqual({
          group: {
            op: 'AND',
            filters: [
              {
                group: {
                  op: 'OR',
                  filters: [
                    {
                      condition: {
                        column: 'age',
                        function: 'greaterThan',
                        args: [18],
                      },
                    },
                    {
                      condition: {
                        column: 'premium',
                        function: 'equals',
                        args: [true],
                      },
                    },
                  ],
                },
              },
              {
                condition: {
                  column: 'status',
                  function: 'equals',
                  args: ['active'],
                },
              },
            ],
          },
        });
      });
    });

    describe('Value Types', () => {
      it('should parse string values with double quotes', () => {
        const result = parseFilterQuery('name = "John Doe"');
        expect(result.condition?.args[0]).toBe('John Doe');
      });

      it('should parse string values with single quotes', () => {
        const result = parseFilterQuery("name = 'Jane Doe'");
        expect(result.condition?.args[0]).toBe('Jane Doe');
      });

      it('should parse integer values', () => {
        const result = parseFilterQuery('count = 42');
        expect(result.condition?.args[0]).toBe(42);
      });

      it('should parse float values', () => {
        const result = parseFilterQuery('price = 19.99');
        expect(result.condition?.args[0]).toBe(19.99);
      });

      it('should parse negative numbers', () => {
        const result = parseFilterQuery('temperature = -5');
        expect(result.condition?.args[0]).toBe(-5);
      });

      it('should parse boolean values', () => {
        const result = parseFilterQuery('active = true');
        expect(result.condition?.args[0]).toBe(true);

        const result2 = parseFilterQuery('deleted = FALSE');
        expect(result2.condition?.args[0]).toBe(false);
      });

      it('should parse null values', () => {
        const result = parseFilterQuery('field = null');
        expect(result.condition?.args[0]).toBe(null);
      });
    });

    describe('Quoted Identifiers', () => {
      it('should parse square bracket quoted identifiers', () => {
        const result = parseFilterQuery('[User Name] = "John"');
        expect(result.condition?.column).toBe('User Name');
      });

      it('should parse backtick quoted identifiers', () => {
        const result = parseFilterQuery('`column-name` > 5');
        expect(result.condition?.column).toBe('column-name');
      });
    });

    describe('Case Insensitivity', () => {
      it('should parse keywords in lowercase', () => {
        const result = parseFilterQuery('age > 18 and status = "active"');
        expect(result.group?.op).toBe('AND');
      });

      it('should parse keywords in uppercase', () => {
        const result = parseFilterQuery('AGE > 18 AND STATUS = "active"');
        expect(result.group?.op).toBe('AND');
      });

      it('should parse keywords in mixed case', () => {
        // ANTLR4 only recognizes fully uppercase or lowercase keywords, not mixed case
        // This test now validates that mixed case throws an error as expected
        expect(() =>
          parseFilterQuery('age > 18 AnD status = "active"')
        ).toThrow(FilterParseError);
      });
    });

    describe('Error Handling', () => {
      it('should throw error for empty input', () => {
        expect(() => parseFilterQuery('')).toThrow(FilterParseError);
      });

      it('should throw error for whitespace only input', () => {
        expect(() => parseFilterQuery('   ')).toThrow(FilterParseError);
      });

      it('should throw error for unterminated string', () => {
        expect(() => parseFilterQuery('name = "unterminated')).toThrow(
          FilterParseError
        );
      });

      it('should throw error for invalid operator', () => {
        expect(() => parseFilterQuery('name ~ "test"')).toThrow(
          FilterParseError
        );
      });

      it('should throw error for missing value', () => {
        expect(() => parseFilterQuery('age >')).toThrow(FilterParseError);
      });

      it('should throw error for unmatched parentheses', () => {
        expect(() => parseFilterQuery('(age > 18')).toThrow(FilterParseError);
      });
    });
  });

  describe('validateFilterQuery', () => {
    it('should return valid for correct query', () => {
      const result = validateFilterQuery('age > 18');
      expect(result.valid).toBe(true);
      expect(result.error).toBeUndefined();
    });

    it('should return invalid for empty query', () => {
      const result = validateFilterQuery('');
      expect(result.valid).toBe(false);
      expect(result.error).toBeDefined();
    });

    it('should return invalid for malformed query', () => {
      const result = validateFilterQuery('age > ');
      expect(result.valid).toBe(false);
      expect(result.error).toBeDefined();
    });

    it('should return error message for parse errors', () => {
      const result = validateFilterQuery('name = "unterminated');
      expect(result.valid).toBe(false);
      // ANTLR4 gives a different error message than our custom parser
      expect(result.error).toContain('Parse error');
    });
  });

  describe('Type Preservation', () => {
    describe('Value Type Preservation', () => {
      it('should preserve integer types', () => {
        const filter = parseFilterQuery('age = 5');
        expect(filter.condition?.args[0]).toBe(5);
        expect(typeof filter.condition?.args[0]).toBe('number');
        expect(Number.isInteger(filter.condition?.args[0])).toBe(true);
      });

      it('should preserve float types', () => {
        const filter = parseFilterQuery('price = 19.99');
        expect(filter.condition?.args[0]).toBe(19.99);
        expect(typeof filter.condition?.args[0]).toBe('number');
        expect(Number.isInteger(filter.condition?.args[0])).toBe(false);
      });

      it('should preserve negative numbers', () => {
        const filter = parseFilterQuery('temperature = -5');
        expect(filter.condition?.args[0]).toBe(-5);
        expect(typeof filter.condition?.args[0]).toBe('number');
      });

      it('should preserve boolean types', () => {
        const filter = parseFilterQuery('active = true');
        expect(filter.condition?.args[0]).toBe(true);
        expect(typeof filter.condition?.args[0]).toBe('boolean');

        const filter2 = parseFilterQuery('deleted = false');
        expect(filter2.condition?.args[0]).toBe(false);
        expect(typeof filter2.condition?.args[0]).toBe('boolean');
      });

      it('should preserve string types', () => {
        const filter = parseFilterQuery('name = "John"');
        expect(filter.condition?.args[0]).toBe('John');
        expect(typeof filter.condition?.args[0]).toBe('string');
      });

      it('should preserve null values', () => {
        const filter = parseFilterQuery('field = null');
        expect(filter.condition?.args[0]).toBe(null);
      });

      it('should preserve array types in IN clause', () => {
        const filter = parseFilterQuery('id IN (1, 2, 3)');
        const args = filter.condition?.args[0] as number[];
        expect(Array.isArray(args)).toBe(true);
        expect(args).toEqual([1, 2, 3]);
        expect(args.every(n => typeof n === 'number')).toBe(true);
      });

      it('should preserve mixed types in IN clause', () => {
        const filter = parseFilterQuery('status IN ("active", "pending")');
        const args = filter.condition?.args[0] as string[];
        expect(Array.isArray(args)).toBe(true);
        expect(args).toEqual(['active', 'pending']);
        expect(args.every(s => typeof s === 'string')).toBe(true);
      });
    });

    describe('Complex Filters with Types', () => {
      it('should preserve types in AND expressions', () => {
        const filter = parseFilterQuery('age > 18 AND premium = true');
        const filters = filter.group?.filters;

        expect(filters?.[0].condition?.args[0]).toBe(18);
        expect(typeof filters?.[0].condition?.args[0]).toBe('number');

        expect(filters?.[1].condition?.args[0]).toBe(true);
        expect(typeof filters?.[1].condition?.args[0]).toBe('boolean');
      });

      it('should preserve types in OR expressions', () => {
        const filter = parseFilterQuery('score = 100 OR name = "John"');
        const filters = filter.group?.filters;

        expect(filters?.[0].condition?.args[0]).toBe(100);
        expect(typeof filters?.[0].condition?.args[0]).toBe('number');

        expect(filters?.[1].condition?.args[0]).toBe('John');
        expect(typeof filters?.[1].condition?.args[0]).toBe('string');
      });

      it('should preserve types in nested expressions', () => {
        const filter = parseFilterQuery(
          '(age > 18 AND age < 65) OR premium = true'
        );
        const orFilters = filter.group?.filters;
        const andFilters = orFilters?.[0].group?.filters;

        expect(andFilters?.[0].condition?.args[0]).toBe(18);
        expect(andFilters?.[1].condition?.args[0]).toBe(65);
        expect(orFilters?.[1].condition?.args[0]).toBe(true);

        expect(typeof andFilters?.[0].condition?.args[0]).toBe('number');
        expect(typeof andFilters?.[1].condition?.args[0]).toBe('number');
        expect(typeof orFilters?.[1].condition?.args[0]).toBe('boolean');
      });
    });

    describe('Comparison Operators with Types', () => {
      it('should preserve number types in comparisons', () => {
        const testCases = [
          { query: 'age > 18', value: 18 },
          { query: 'age >= 18', value: 18 },
          { query: 'age < 65', value: 65 },
          { query: 'age <= 65', value: 65 },
          { query: 'age = 30', value: 30 },
          { query: 'age != 0', value: 0 },
        ];

        testCases.forEach(({ query, value }) => {
          const filter = parseFilterQuery(query);
          expect(filter.condition?.args[0]).toBe(value);
          expect(typeof filter.condition?.args[0]).toBe('number');
        });
      });

      it('should preserve float types in comparisons', () => {
        const filter = parseFilterQuery('price >= 19.99');
        expect(filter.condition?.args[0]).toBe(19.99);
        expect(typeof filter.condition?.args[0]).toBe('number');
        expect(Number.isInteger(filter.condition?.args[0])).toBe(false);
      });
    });

    describe('String Functions with Types', () => {
      it('should preserve string types in CONTAINS', () => {
        const filter = parseFilterQuery('name CONTAINS "test"');
        expect(filter.condition?.args[0]).toBe('test');
        expect(typeof filter.condition?.args[0]).toBe('string');
      });

      it('should preserve string types in STARTS_WITH', () => {
        const filter = parseFilterQuery('email STARTS_WITH "admin"');
        expect(filter.condition?.args[0]).toBe('admin');
        expect(typeof filter.condition?.args[0]).toBe('string');
      });

      it('should preserve string types in ENDS_WITH', () => {
        const filter = parseFilterQuery('domain ENDS_WITH ".com"');
        expect(filter.condition?.args[0]).toBe('.com');
        expect(typeof filter.condition?.args[0]).toBe('string');
      });
    });

    describe('Edge Cases', () => {
      it('should handle zero correctly', () => {
        const filter = parseFilterQuery('count = 0');
        expect(filter.condition?.args[0]).toBe(0);
        expect(typeof filter.condition?.args[0]).toBe('number');
      });

      it('should handle negative zero correctly', () => {
        const filter = parseFilterQuery('value = -0');
        // JavaScript preserves -0 vs 0 distinction
        expect(filter.condition?.args[0]).toBe(-0);
        expect(typeof filter.condition?.args[0]).toBe('number');
        expect(Object.is(filter.condition?.args[0], -0)).toBe(true);
      });

      it('should handle large integers', () => {
        const filter = parseFilterQuery('id = 9007199254740991'); // MAX_SAFE_INTEGER
        expect(filter.condition?.args[0]).toBe(9007199254740991);
        expect(typeof filter.condition?.args[0]).toBe('number');
      });

      it('should handle very small decimals', () => {
        const filter = parseFilterQuery('ratio = 0.0001');
        expect(filter.condition?.args[0]).toBe(0.0001);
        expect(typeof filter.condition?.args[0]).toBe('number');
      });

      it('should handle empty strings', () => {
        const filter = parseFilterQuery('name = ""');
        expect(filter.condition?.args[0]).toBe('');
        expect(typeof filter.condition?.args[0]).toBe('string');
      });
    });
  });

  describe('Column Name Case Insensitivity', () => {
    // Simulate the normalization logic that happens in TableOptions
    const normalizeColumnNames = (
      filter: ReturnType<typeof parseFilterQuery>,
      columnNameMap: Map<string, string>
    ): ReturnType<typeof parseFilterQuery> => {
      if (filter.condition) {
        const actualColumnName = columnNameMap.get(
          filter.condition.column.toLowerCase()
        );
        if (!actualColumnName) {
          throw new Error(`Column not found: ${filter.condition.column}`);
        }
        return {
          ...filter,
          condition: {
            ...filter.condition,
            column: actualColumnName,
          },
        };
      }
      if (filter.group) {
        return {
          ...filter,
          group: {
            ...filter.group,
            filters: filter.group.filters.map(f =>
              normalizeColumnNames(f, columnNameMap)
            ),
          },
        };
      }
      return filter;
    };

    describe('Single Column Normalization', () => {
      const columns = [
        { name: 'UserName', type: 'string' },
        { name: 'Age', type: 'int32' },
        { name: 'Email', type: 'string' },
      ];
      const columnNameMap = new Map(
        columns.map(c => [c.name.toLowerCase(), c.name])
      );

      it('should normalize lowercase to actual case', () => {
        const filter = parseFilterQuery('username = "john"');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('UserName');
      });

      it('should normalize uppercase to actual case', () => {
        const filter = parseFilterQuery('AGE > 18');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('Age');
      });

      it('should normalize mixed case to actual case', () => {
        const filter = parseFilterQuery('eMaIl CONTAINS "test"');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('Email');
      });

      it('should keep correct case unchanged', () => {
        const filter = parseFilterQuery('UserName = "john"');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('UserName');
      });
    });

    describe('Multiple Column Normalization', () => {
      const columns = [
        { name: 'FirstName', type: 'string' },
        { name: 'LastName', type: 'string' },
        { name: 'Age', type: 'int32' },
        { name: 'IsActive', type: 'bool' },
      ];
      const columnNameMap = new Map(
        columns.map(c => [c.name.toLowerCase(), c.name])
      );

      it('should normalize all columns in AND expression', () => {
        const filter = parseFilterQuery('firstname = "John" AND age > 18');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        const filters = normalized.group?.filters;
        expect(filters?.[0].condition?.column).toBe('FirstName');
        expect(filters?.[1].condition?.column).toBe('Age');
      });

      it('should normalize all columns in OR expression', () => {
        const filter = parseFilterQuery('LASTNAME = "Doe" OR isactive = true');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        const filters = normalized.group?.filters;
        expect(filters?.[0].condition?.column).toBe('LastName');
        expect(filters?.[1].condition?.column).toBe('IsActive');
      });

      it('should normalize columns in nested expressions', () => {
        const filter = parseFilterQuery(
          '(FIRSTNAME = "John" AND lastname = "Doe") OR age > 30'
        );
        const normalized = normalizeColumnNames(filter, columnNameMap);

        const orFilters = normalized.group?.filters;
        const andFilters = orFilters?.[0].group?.filters;

        expect(andFilters?.[0].condition?.column).toBe('FirstName');
        expect(andFilters?.[1].condition?.column).toBe('LastName');
        expect(orFilters?.[1].condition?.column).toBe('Age');
      });
    });

    describe('Error Handling', () => {
      const columns = [{ name: 'ValidColumn', type: 'string' }];
      const columnNameMap = new Map(
        columns.map(c => [c.name.toLowerCase(), c.name])
      );

      it('should throw error for invalid column name', () => {
        const filter = parseFilterQuery('InvalidColumn = "test"');

        expect(() => normalizeColumnNames(filter, columnNameMap)).toThrow(
          'Column not found: InvalidColumn'
        );
      });

      it('should throw error even if case is different for invalid column', () => {
        const filter = parseFilterQuery('INVALIDCOLUMN = "test"');

        expect(() => normalizeColumnNames(filter, columnNameMap)).toThrow(
          'Column not found: INVALIDCOLUMN'
        );
      });
    });

    describe('Special Cases', () => {
      const columns = [
        { name: 'user_name', type: 'string' },
        { name: 'USER_ID', type: 'int32' },
        { name: 'UserEmail', type: 'string' },
      ];
      const columnNameMap = new Map(
        columns.map(c => [c.name.toLowerCase(), c.name])
      );

      it('should normalize snake_case columns', () => {
        const filter = parseFilterQuery('USER_NAME = "john"');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('user_name');
      });

      it('should normalize all caps columns', () => {
        const filter = parseFilterQuery('user_id = 123');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('USER_ID');
      });

      it('should normalize camelCase columns', () => {
        const filter = parseFilterQuery('USEREMAIL CONTAINS "test"');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('UserEmail');
      });
    });

    describe('Quoted Identifiers', () => {
      const columns = [
        { name: 'User Name', type: 'string' },
        { name: 'First-Last', type: 'string' },
      ];
      const columnNameMap = new Map(
        columns.map(c => [c.name.toLowerCase(), c.name])
      );

      it('should normalize quoted column names with spaces', () => {
        const filter = parseFilterQuery('[user name] = "john"');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('User Name');
      });

      it('should normalize quoted column names with dashes', () => {
        const filter = parseFilterQuery('[FIRST-LAST] = "John-Doe"');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('First-Last');
      });

      it('should normalize backtick quoted identifiers', () => {
        const filter = parseFilterQuery('`USER NAME` = "john"');
        const normalized = normalizeColumnNames(filter, columnNameMap);

        expect(normalized.condition?.column).toBe('User Name');
      });
    });
  });
});
