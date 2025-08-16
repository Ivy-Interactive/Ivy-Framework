using System.Text;

namespace Ivy.Tui
{
    public class AnsiTable
    {
        private readonly List<string> _columns = new();
        private readonly List<List<string>> _rows = new();

        public void AddColumn(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Column name cannot be null or empty.", nameof(name));

            if (_rows.Count > 0)
                throw new InvalidOperationException("Cannot add columns after rows have been added.");

            _columns.Add(name);
        }

        public void AddRow(params string[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (_columns.Count == 0)
                throw new InvalidOperationException("No columns have been defined. Add columns before adding rows.");

            if (values.Length != _columns.Count)
                throw new ArgumentException($"Row must have {_columns.Count} values to match the number of columns, but got {values.Length}.", nameof(values));

            _rows.Add(values.Select(v => v ?? string.Empty).ToList());
        }

        public string Render()
        {
            if (_columns.Count == 0)
                return string.Empty;

            var columnWidths = CalculateColumnWidths();
            var sb = new StringBuilder();

            RenderTopBorder(sb, columnWidths);
            RenderHeaderRow(sb, columnWidths);
            RenderMiddleBorder(sb, columnWidths);

            foreach (var row in _rows)
            {
                RenderDataRow(sb, row, columnWidths);
            }

            RenderBottomBorder(sb, columnWidths);

            return sb.ToString();
        }

        private int[] CalculateColumnWidths()
        {
            var widths = new int[_columns.Count];

            for (int i = 0; i < _columns.Count; i++)
            {
                widths[i] = _columns[i].Length;

                foreach (var row in _rows)
                {
                    if (i < row.Count)
                    {
                        widths[i] = Math.Max(widths[i], row[i].Length);
                    }
                }

                widths[i] = Math.Min(widths[i] + 2, 50);
            }

            return widths;
        }

        private void RenderTopBorder(StringBuilder sb, int[] columnWidths)
        {
            sb.Append('┌');
            for (int i = 0; i < columnWidths.Length; i++)
            {
                sb.Append(new string('─', columnWidths[i]));
                if (i < columnWidths.Length - 1)
                    sb.Append('┬');
            }
            sb.AppendLine("┐");
        }

        private void RenderMiddleBorder(StringBuilder sb, int[] columnWidths)
        {
            sb.Append('├');
            for (int i = 0; i < columnWidths.Length; i++)
            {
                sb.Append(new string('─', columnWidths[i]));
                if (i < columnWidths.Length - 1)
                    sb.Append('┼');
            }
            sb.AppendLine("┤");
        }

        private void RenderBottomBorder(StringBuilder sb, int[] columnWidths)
        {
            sb.Append('└');
            for (int i = 0; i < columnWidths.Length; i++)
            {
                sb.Append(new string('─', columnWidths[i]));
                if (i < columnWidths.Length - 1)
                    sb.Append('┴');
            }
            sb.AppendLine("┘");
        }

        private void RenderHeaderRow(StringBuilder sb, int[] columnWidths)
        {
            sb.Append('│');
            for (int i = 0; i < _columns.Count; i++)
            {
                var text = _columns[i];
                var padding = columnWidths[i] - text.Length;
                var leftPad = padding / 2;
                var rightPad = padding - leftPad;

                sb.Append(new string(' ', leftPad));
                sb.Append(text);
                sb.Append(new string(' ', rightPad));
                sb.Append('│');
            }
            sb.AppendLine();
        }

        private void RenderDataRow(StringBuilder sb, List<string> row, int[] columnWidths)
        {
            sb.Append('│');
            for (int i = 0; i < _columns.Count; i++)
            {
                var text = i < row.Count ? row[i] : string.Empty;
                if (text.Length > columnWidths[i] - 2)
                {
                    text = text[..(columnWidths[i] - 5)] + "...";
                }

                sb.Append(' ');
                sb.Append(text);
                sb.Append(new string(' ', columnWidths[i] - text.Length - 1));
                sb.Append('│');
            }
            sb.AppendLine();
        }
    }
}