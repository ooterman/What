using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace GlobalCommon
{
    public class ExcelHelper
    {
		/// <summary>
		/// 保存Excel文件
		/// <para>Excel的导入导出都会在服务器生成一个文件</para>
		/// <para>路径：UpFiles/ExcelFiles</para>
		/// </summary>
		/// <param name="file">传入的文件对象</param>
		/// <returns>如果保存成功则返回文件的位置;如果保存失败则返回空</returns>
		//public static string SaveExcelFile(HttpFile file)
		//{
		//    try
		//    {
		//        var fileName = file.Name.Insert(file.Name.LastIndexOf('.'), "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
		//        var filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/UpFiles/ExcelFiles"), fileName);
		//        string directoryName = Path.GetDirectoryName(filePath);
		//        if (!Directory.Exists(directoryName))
		//        {
		//            Directory.CreateDirectory(directoryName);
		//        }
		//        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
		//        {
		//            file.Value.CopyTo(fileStream);
		//        }
		//        return filePath;
		//    }
		//    catch
		//    {
		//        return string.Empty;
		//    }
		//}

		public static string WriteDataToExcel(List<string> headerNameList, List<Hashtable> htlist, string sheetName)
        {
			DataTable dt = CommonHelper.Convert2DataTable(htlist);
			return WriteDataToExcel(headerNameList, dt, sheetName);

		}
		public static string WriteDataToExcel(List<string> headerNameList,DataTable dt, string sheetName)
		{
			bool flag = dt == null;
			string result;
			if (flag)
			{
				result = "";
			}
			else
			{
				IWorkbook workbook = new HSSFWorkbook();
				string arg = "xls";
	
				ISheet sheet = workbook.CreateSheet(sheetName);
				IRow row = sheet.CreateRow(0);
				for (int i = 0; i < headerNameList.Count; i++)
				{
					ICell cell = row.CreateCell(i);
					cell.SetCellValue(string.IsNullOrWhiteSpace(headerNameList[i]) ? string.Empty : headerNameList[i]);
				}
				fillDataToExcel(workbook, sheet, dt, null, null, 1, headerNameList);
				string text = string.Format("{0}\\{1}.{2}", CommonHelper.GetPath( "temporary"), Guid.NewGuid().ToString(), arg);
				FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.Write);
				workbook.Write(fileStream);
				fileStream.Close();
				fileStream.Dispose();
				result = text;
			}
			return result;
		}

	
		private static void fillDataToExcel(IWorkbook workbook, ISheet sheet, DataTable mainTable, DataTable subTable, string subTableFK, int startIndex, List<string> headerNameList)
		{
			int num = startIndex;
			ICellStyle cellStyle = workbook.CreateCellStyle();
			IDataFormat dataFormat = workbook.CreateDataFormat();
			cellStyle.DataFormat = dataFormat.GetFormat("yyyy-mm-dd");
			int num2 = mainTable.Columns.Count - 1;
			string text = (mainTable.PrimaryKey == null || mainTable.PrimaryKey.Length == 0) ? "" : mainTable.PrimaryKey[0].ColumnName;
			foreach (object obj in mainTable.Rows)
			{
				DataRow dataRow = (DataRow)obj;
				int num3 = 0;
				bool flag = subTable != null && !string.IsNullOrWhiteSpace(subTableFK) && !string.IsNullOrWhiteSpace(text);
				IRow row;
				if (flag)
				{
					DataRow[] array = subTable.Select(string.Format("{0}='{1}'", subTableFK, dataRow[text]));
					foreach (DataRow dataRow2 in array)
					{
						row = getRow(sheet, num + num3);
						for (int j = 0; j < subTable.Columns.Count; j++)
						{
							bool flag2 = subTable.Columns[j].ColumnName == subTableFK;
							if (!flag2)
							{
								ICell cell = getCell(row, j + num2);
								Type dataType = subTable.Columns[j].DataType;
								setCellValue(cell, dataType, cellStyle, dataRow2[j]);
							}
						}
						num3++;
					}
				}
				num3 = ((num3 > 0) ? (num3 - 1) : 0);
				row = getRow(sheet, num);
				for (int k = 0; k < headerNameList.Count; k++)
				{
					ICell cell = mergeCell(sheet, num, num3, k, 0);
					Type dataType = mainTable.Columns[k].DataType;
					setCellValue(cell, dataType, cellStyle, dataRow[k]);
				}
				num += num3 + 1;
			}
		}

		private static void setCellValue(ICell cell, Type type, ICellStyle dateStyle, object valueObj)
		{
			bool flag = type.Equals(typeof(bool));
			if (flag)
			{
				bool flag2 = valueObj == DBNull.Value;
				if (flag2)
				{
					cell.SetCellValue(string.Empty);
				}
				else
				{
					cell.SetCellValue(Convert.ToBoolean(valueObj));
				}
				cell.SetCellType(CellType.Boolean);
			}
			else
			{
				bool flag3 = type.Equals(typeof(DateTime));
				if (flag3)
				{
					bool flag4 = valueObj == DBNull.Value;
					if (flag4)
					{
						cell.SetCellValue(string.Empty);
					}
					else
					{
						bool flag5 = Convert.ToDateTime(valueObj).Year == 1;
						if (flag5)
						{
							cell.SetCellValue(string.Empty);
						}
						else
						{
							cell.SetCellValue(Convert.ToDateTime(valueObj));
							cell.CellStyle = dateStyle;
						}
					}
				}
				else
				{
					bool flag6 = type.Equals(typeof(double)) || type.Equals(typeof(float)) || type.Equals(typeof(decimal)) || type.Equals(typeof(byte)) || type.Equals(typeof(sbyte)) || type.Equals(typeof(short)) || type.Equals(typeof(ushort)) || type.Equals(typeof(int)) || type.Equals(typeof(uint)) || type.Equals(typeof(long));
					if (flag6)
					{
						bool flag7 = valueObj == DBNull.Value;
						if (flag7)
						{
							cell.SetCellValue(string.Empty);
						}
						else
						{
							cell.SetCellValue(Convert.ToDouble(valueObj));
						}
						cell.SetCellType(CellType.Numeric);
					}
					else
					{
						cell.SetCellValue(string.Format("{0}", valueObj));
						cell.SetCellType(CellType.String);
					}
				}
			}
		}

		private static IRow getRow(ISheet sheet, int rowIndex)
		{
			IRow row = sheet.GetRow(rowIndex);
			bool flag = row == null && rowIndex >= 0;
			if (flag)
			{
				row = sheet.CreateRow(rowIndex);
			}
			return row;
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x0002134C File Offset: 0x0001F54C
		private static ICell getCell(IRow row, int columnIndex)
		{
			ICell cell = row.GetCell(columnIndex);
			bool flag = cell == null && columnIndex >= 0;
			if (flag)
			{
				cell = row.CreateCell(columnIndex);
			}
			return cell;
		}
		private static ICell mergeCell(ISheet sheet, int startRowIndex, int rowCount, int startColumnIndex, int columnCount)
		{
			bool flag = rowCount <= 0;
			ICell result;
			if (flag)
			{
				result = getCell(sheet.GetRow(startRowIndex), startColumnIndex);
			}
			else
			{
				bool flag2 = columnCount < 0;
				if (flag2)
				{
					columnCount = 0;
				}
				sheet.AddMergedRegion(new CellRangeAddress(startRowIndex, startRowIndex + rowCount, startColumnIndex, startColumnIndex + columnCount));
				IRow row = getRow(sheet, startRowIndex);
				ICell cell = getCell(row, startColumnIndex);
				result = cell;
			}
			return result;
		}


	}
}