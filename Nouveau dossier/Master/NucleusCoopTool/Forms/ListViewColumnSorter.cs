using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ListViewSorter
{
    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        public enum SortModifiers
        {
            SortByImage,
            SortByCheckbox,
            SortByText
        }

        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        public int ColumnToSort;
        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        public SortOrder OrderOfSort;
        /// <summary>
        /// Case insensitive comparer object
        /// </summary>

        private NumberCaseInsensitiveComparer ObjectCompare;
        private ImageTextComparer FirstObjectCompare;
        private CheckboxTextComparer FirstObjectCompare2;

        private SortModifiers mySortModifier = SortModifiers.SortByText;
        public SortModifiers _SortModifier
        {
            set => mySortModifier = value;
            get => mySortModifier;
        }

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            ColumnToSort = 0;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new NumberCaseInsensitiveComparer();
            FirstObjectCompare = new ImageTextComparer();
            FirstObjectCompare2 = new CheckboxTextComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int compareResult = 0;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            ListView listViewMain = listviewX.ListView;

            // Calculate correct return value based on object comparison
            if (listViewMain.Sorting != SortOrder.Ascending &&
                listViewMain.Sorting != SortOrder.Descending)
            {
                // Return '0' to indicate they are equal
                return compareResult;
            }

            if (mySortModifier.Equals(SortModifiers.SortByText) || ColumnToSort > 0)
            {
                // Compare the two items

                if (listviewX.SubItems.Count <= ColumnToSort &&
                    listviewY.SubItems.Count <= ColumnToSort)
                {
                    compareResult = ObjectCompare.Compare(null, null);
                }
                else if (listviewX.SubItems.Count <= ColumnToSort &&
                         listviewY.SubItems.Count > ColumnToSort)
                {
                    compareResult = ObjectCompare.Compare(null, listviewY.SubItems[ColumnToSort].Text.Trim());
                }
                else if (listviewX.SubItems.Count > ColumnToSort && listviewY.SubItems.Count <= ColumnToSort)
                {
                    compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text.Trim(), null);
                }
                else
                {
                    compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text.Trim(), listviewY.SubItems[ColumnToSort].Text.Trim());
                }
            }
            else
            {
                switch (mySortModifier)
                {
                    case SortModifiers.SortByCheckbox:
                        compareResult = FirstObjectCompare2.Compare(x, y);
                        break;
                    case SortModifiers.SortByImage:
                        compareResult = FirstObjectCompare.Compare(x, y);
                        break;
                    default:
                        compareResult = FirstObjectCompare.Compare(x, y);
                        break;
                }
            }

            // Calculate correct return value based on object comparison
            if (OrderOfSort == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                return (-compareResult);
            }
            else
            {
                // Return '0' to indicate they are equal
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set => ColumnToSort = value;
            get => ColumnToSort;
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set => OrderOfSort = value;
            get => OrderOfSort;
        }

    }

    public class ImageTextComparer : IComparer
    {
        //private CaseInsensitiveComparer ObjectCompare;
        private NumberCaseInsensitiveComparer ObjectCompare;

        public ImageTextComparer()
        {
            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new NumberCaseInsensitiveComparer();
        }

        public int Compare(object x, object y)
        {
            //int compareResult;
            int image1, image2;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            image1 = listviewX.ImageIndex;
            listviewY = (ListViewItem)y;
            image2 = listviewY.ImageIndex;

            if (image1 < image2)
            {
                return -1;
            }
            else if (image1 == image2)
            {
                return ObjectCompare.Compare(listviewX.Text.Trim(), listviewY.Text.Trim());
            }
            else
            {
                return 1;
            }
        }
    }

    public class CheckboxTextComparer : IComparer
    {
        private NumberCaseInsensitiveComparer ObjectCompare;

        public CheckboxTextComparer()
        {
            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new NumberCaseInsensitiveComparer();
        }

        public int Compare(object x, object y)
        {
            // Cast the objects to be compared to ListViewItem objects
            ListViewItem listviewX = (ListViewItem)x;
            ListViewItem listviewY = (ListViewItem)y;

            if (listviewX.Checked && !listviewY.Checked)
            {
                return -1;
            }
            else if (listviewX.Checked.Equals(listviewY.Checked))
            {
                if (listviewX.ImageIndex < listviewY.ImageIndex)
                {
                    return -1;
                }
                else if (listviewX.ImageIndex == listviewY.ImageIndex)
                {
                    return ObjectCompare.Compare(listviewX.Text.Trim(), listviewY.Text.Trim());
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 1;
            }
        }
    }

    public class NumberCaseInsensitiveComparer : CaseInsensitiveComparer
    {
        public NumberCaseInsensitiveComparer()
        {

        }

        public new int Compare(object x, object y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return -1;
            }
            else if (x != null && y == null)
            {
                return 1;
            }

            if ((x is string) && IsDecimalNumber((string)x) && (y is string) && IsDecimalNumber((string)y))
            {
                try
                {
                    decimal xx = decimal.Parse(((string)x).Trim());
                    decimal yy = decimal.Parse(((string)y).Trim());

                    return base.Compare(xx, yy);
                }
                catch
                {
                    return -1;
                }
            }
            else
            {
                return base.Compare(x, y);
            }
        }

        // deprecated
        //private bool IsWholeNumber(string strNumber)
        //{
        //    Regex wholePattern = new Regex(@"^\d+$");
        //    return wholePattern.IsMatch(strNumber);
        //}

        private string GetNumberDecimalSeparator()
        {
            return System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        }

        // http://stackoverflow.com/questions/4246077/matching-numbers-with-regular-expressions-only-digits-and-commas/4247184#4247184
        // https://www.debuggex.com/r/Lyx0F0y1LORvNhwA
        private bool IsDecimalNumber(string strNumber)
        {
            //@"^-?(\d+|(\d{1,3}((,|\.)\d{3})*))((,|\.)\d+)?$"

            //string regex = @"^-?(?:(?:0|[1-9][0-9]*)(?:" + GetNumberDecimalSeparator() + @"[0-9]+)?|[1-9][0-9]{1,2}(?:,[0-9]{3})+)$";

            string regex = @"^-?(\d+|(\d{1,3}((,|\.)\d{3})*))((,|\.)\d+)?$";

            Regex wholePattern = new Regex(regex);
            return wholePattern.IsMatch(strNumber);
        }
    }

}
