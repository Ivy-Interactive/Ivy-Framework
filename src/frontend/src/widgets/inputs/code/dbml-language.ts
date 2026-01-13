import { StreamLanguage } from '@codemirror/language';

const dbmlMode = {
  startState: () => ({}),
  // eslint-disable-next-line @typescript-eslint/no-explicit-any,
  token: (stream: any) => {
    if (stream.match(/\/\//)) {
      stream.skipToEnd();
      return 'comment';
    }

    if (stream.match(/"(.*?)"/) || stream.match(/'(.*?)'/)) {
      return 'string';
    }

    if (stream.match(/\b(Table|Ref|Enum|Project|TableGroup|Note)\b/i)) {
      return 'keyword';
    }

    if (stream.match(/\b(int|uuid|varchar|boolean|text|datetime)\b/i)) {
      return 'typeName';
    }

    if (stream.match(/\b(primary key|not null|unique|increment)\b/i)) {
      return 'attribute';
    }

    if (stream.match(/[{}[\](),;]/)) {
      return 'bracket';
    }

    if (stream.match(/[a-zA-Z_][\w-]*/)) {
      return 'variableName';
    }

    stream.next();
    return null;
  },
};

// Database Markup Language (DBML) support
export const dbml = () => StreamLanguage.define(dbmlMode);
