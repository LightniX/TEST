using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using param;
using param_player;
using UnityEngine;

// Token: 0x020002E8 RID: 744
public class MaidParam : MonoBehaviour
{
	// Token: 0x17000303 RID: 771
	// (get) Token: 0x060016F4 RID: 5876 RVA: 0x000A8C28 File Offset: 0x000A6E28
	// (set) Token: 0x060016F3 RID: 5875 RVA: 0x000A8C20 File Offset: 0x000A6E20
	public static int kYotogiClassIdEnabledMax { get; private set; }

	// Token: 0x17000304 RID: 772
	// (get) Token: 0x060016F5 RID: 5877 RVA: 0x000A8C30 File Offset: 0x000A6E30
	public static int kYotogiClassIdMax
	{
		get
		{
			return 64;
		}
	}

	// Token: 0x060016F6 RID: 5878 RVA: 0x000A8C34 File Offset: 0x000A6E34
	public static int GetYotogiClassIdFromName(string yotogi_class_name)
	{
		MaidParam.CreateMaidClassAndYotogiClassNameMap();
		NDebug.Assert(MaidParam.yotogi_class_name_map_.ContainsKey(yotogi_class_name), "夜伽クラス[" + yotogi_class_name + "]は不明なクラスです");
		return MaidParam.yotogi_class_name_map_[yotogi_class_name];
	}

	// Token: 0x060016F7 RID: 5879 RVA: 0x000A8C74 File Offset: 0x000A6E74
	public static string GetYotogiClassNameFromId(int yotogi_class_id)
	{
		MaidParam.CreateMaidClassAndYotogiClassNameMap();
		return MaidParam.yotogi_class_id_map_[yotogi_class_id];
	}

	// Token: 0x060016F8 RID: 5880 RVA: 0x000A8C88 File Offset: 0x000A6E88
	public static bool EnabledMaidClass(MaidClassType check_maid_class)
	{
		MaidParam.CreateMaidClassAndYotogiClassEnabled();
		return MaidParam.maid_class_enabled_.ContainsKey(check_maid_class);
	}

	// Token: 0x060016F9 RID: 5881 RVA: 0x000A8C9C File Offset: 0x000A6E9C
	public static string GetNameMaidClass(MaidClassType maid_class)
	{
		return MaidParam.maid_class_enabled_[maid_class];
	}

	// Token: 0x060016FA RID: 5882 RVA: 0x000A8CAC File Offset: 0x000A6EAC
	public static bool EnabledYotogiClass(int check_yotogi_class)
	{
		MaidParam.CreateMaidClassAndYotogiClassEnabled();
		return MaidParam.yotogi_class_enabled_.ContainsKey(check_yotogi_class);
	}

	// Token: 0x060016FB RID: 5883 RVA: 0x000A8CC0 File Offset: 0x000A6EC0
	public static string GetNameYotogiClass(int yotogi_class)
	{
		return MaidParam.yotogi_class_enabled_[yotogi_class];
	}

	// Token: 0x060016FC RID: 5884 RVA: 0x000A8CD0 File Offset: 0x000A6ED0
	public static bool IsEnabledPersonal(Personal check_personal)
	{
		return !string.IsNullOrEmpty(EnumConvert.GetPersonalChar(check_personal));
	}

	// Token: 0x060016FD RID: 5885 RVA: 0x000A8CE0 File Offset: 0x000A6EE0
	private static void CreateMaidClassAndYotogiClassEnabled()
	{
		if (0 < MaidParam.maid_class_enabled_.Count)
		{
			return;
		}
		Action<string> action = delegate(string file_name)
		{
			file_name += ".nei";
			if (!GameUty.FileSystem.IsExistentFile(file_name))
			{
				return;
			}
			using (AFileBase afileBase = GameUty.FileSystem.FileOpen(file_name))
			{
				using (CsvParser csvParser = new CsvParser())
				{
					bool condition = csvParser.Open(afileBase);
					NDebug.Assert(condition, file_name + "\nopen failed.");
					for (int k = 1; k < csvParser.max_cell_y; k++)
					{
						if (csvParser.IsCellToExistData(0, k))
						{
							MaidClassType key = (MaidClassType)((int)Enum.Parse(typeof(MaidClassType), csvParser.GetCellAsString(0, k)));
							if (!MaidParam.maid_class_enabled_.ContainsKey(key))
							{
								MaidParam.maid_class_enabled_.Add(key, csvParser.GetCellAsString(1, k));
							}
						}
					}
				}
			}
		};
		Action<string> action2 = delegate(string file_name)
		{
			file_name += ".nei";
			if (!GameUty.FileSystem.IsExistentFile(file_name))
			{
				return;
			}
			using (AFileBase afileBase = GameUty.FileSystem.FileOpen(file_name))
			{
				using (CsvParser csvParser = new CsvParser())
				{
					bool condition = csvParser.Open(afileBase);
					NDebug.Assert(condition, file_name + "\nopen failed.");
					for (int k = 1; k < csvParser.max_cell_y; k++)
					{
						if (csvParser.IsCellToExistData(0, k))
						{
							int yotogiClassIdFromName = MaidParam.GetYotogiClassIdFromName(csvParser.GetCellAsString(0, k));
							if (!MaidParam.yotogi_class_enabled_.ContainsKey(yotogiClassIdFromName))
							{
								MaidParam.yotogi_class_enabled_.Add(yotogiClassIdFromName, csvParser.GetCellAsString(1, k));
							}
						}
					}
				}
			}
		};
		action("maid_class_enabled_list");
		for (int i = 0; i < GameUty.PathList.Count; i++)
		{
			action("maid_class_enabled_list_" + GameUty.PathList[i]);
		}
		action2("yotogi_class_enabled_list");
		for (int j = 0; j < GameUty.PathList.Count; j++)
		{
			action2("yotogi_class_enabled_list_" + GameUty.PathList[j]);
		}
	}

	// Token: 0x060016FE RID: 5886 RVA: 0x000A8DBC File Offset: 0x000A6FBC
	private static void CreateMaidClassAndYotogiClassNameMap()
	{
		if (0 < MaidParam.yotogi_class_name_map_.Count)
		{
			return;
		}
		using (AFileBase afileBase = GameUty.FileSystem.FileOpen("yotogi_class_infotext.nei"))
		{
			using (CsvParser csvParser = new CsvParser())
			{
				bool condition = csvParser.Open(afileBase);
				NDebug.Assert(condition, "file open error[yotogi_class_infotext.nei]");
				for (int i = 1; i < csvParser.max_cell_y; i++)
				{
					if (csvParser.IsCellToExistData(0, i))
					{
						int cellAsInteger = csvParser.GetCellAsInteger(0, i);
						string cellAsString = csvParser.GetCellAsString(1, i);
						MaidParam.yotogi_class_name_map_.Add(cellAsString, cellAsInteger);
						MaidParam.yotogi_class_id_map_.Add(cellAsInteger, cellAsString);
						if (MaidParam.kYotogiClassIdEnabledMax < cellAsInteger)
						{
							MaidParam.kYotogiClassIdEnabledMax = cellAsInteger;
						}
					}
				}
			}
		}
		MaidParam.kYotogiClassIdEnabledMax++;
	}

	// Token: 0x060016FF RID: 5887 RVA: 0x000A8ED4 File Offset: 0x000A70D4
	public void Awake()
	{
	}

	// Token: 0x06001700 RID: 5888 RVA: 0x000A8ED8 File Offset: 0x000A70D8
	public bool Initialize(Maid maid)
	{
		MaidParam.CreateMaidClassAndYotogiClassEnabled();
		MaidParam.CreateMaidClassAndYotogiClassNameMap();
		Correction.Create();
		this.maid_ = maid;
		this.status_.guid_id = Guid.NewGuid().ToString();
		this.status_.create_time = DateTime.Now.ToString("G", new CultureInfo("ja-JP"));
		string text = this.status_.create_time.Replace("/", string.Empty);
		text = text.Replace(":", string.Empty);
		text = text.Replace(" ", string.Empty);
		this.status_.create_time_num = ulong.Parse(text);
		this.status_.employment_day = GameMain.Instance.CharacterMgr.GetPlayerParam().status.days;
		this.status_.last_name = (this.status_.first_name = string.Empty);
		this.status_.profile = (this.status_.free_comment = string.Empty);
		this.status_.body.cup = string.Empty;
		this.status_.feature = new HashSet<Feature>();
		this.status_.propensity = new HashSet<Propensity>();
		this.status_.skill_data = new Dictionary<int, param.Status.SkillData>();
		this.status_.work_data = new Dictionary<int, param.Status.WorkData>();
		this.status_.generic_flag = new Dictionary<string, int>();
		this.status_.parts_dic = new Dictionary<string, string>();
		this.status_.maid_class_data = new param.Status.MaidClassData<MaidClassType>[32];
		for (int i = 0; i < this.status_.maid_class_data.Length; i++)
		{
			this.status_.maid_class_data[i] = new param.Status.MaidClassData<MaidClassType>();
			this.status_.maid_class_data[i].type = (MaidClassType)i;
		}
		this.status_.yotogi_class_data = new param.Status.MaidClassData<int>[MaidParam.kYotogiClassIdMax];
		for (int j = 0; j < this.status_.yotogi_class_data.Length; j++)
		{
			this.status_.yotogi_class_data[j] = new param.Status.MaidClassData<int>();
			this.status_.yotogi_class_data[j].type = j;
		}
		using (AFileBase afileBase = GameUty.FileSystem.FileOpen("maid_class_exp_list.nei"))
		{
			using (CsvParser csvParser = new CsvParser())
			{
				bool condition = csvParser.Open(afileBase);
				NDebug.Assert(condition, "file open error[maid_class_exp_list.nei]");
				for (int k = 1; k < csvParser.max_cell_y; k++)
				{
					if (csvParser.IsCellToExistData(0, k))
					{
						string cellAsString = csvParser.GetCellAsString(0, k);
						try
						{
							MaidClassType maidClassType = (MaidClassType)((int)Enum.Parse(typeof(MaidClassType), cellAsString));
							List<int> list = new List<int>();
							int num = 1;
							while (csvParser.IsCellToExistData(num, k))
							{
								list.Add(csvParser.GetCellAsInteger(num, k));
								num++;
							}
							this.status_.maid_class_data[(int)maidClassType].exp_system.SetExreienceList(list);
						}
						catch (ArgumentException)
						{
							NDebug.Assert("enum parse error.[" + cellAsString + "]");
						}
					}
				}
			}
		}
		using (AFileBase afileBase2 = GameUty.FileSystem.FileOpen("yotogi_class_exp_list.nei"))
		{
			using (CsvParser csvParser2 = new CsvParser())
			{
				bool condition2 = csvParser2.Open(afileBase2);
				NDebug.Assert(condition2, "file open error[yotogi_class_exp_list.nei]");
				for (int l = 1; l < csvParser2.max_cell_y; l++)
				{
					if (csvParser2.IsCellToExistData(0, l))
					{
						string cellAsString2 = csvParser2.GetCellAsString(0, l);
						try
						{
							int yotogiClassIdFromName = MaidParam.GetYotogiClassIdFromName(cellAsString2);
							List<int> list2 = new List<int>();
							int num2 = 1;
							while (csvParser2.IsCellToExistData(num2, l))
							{
								list2.Add(csvParser2.GetCellAsInteger(num2, l));
								num2++;
							}
							this.status_.yotogi_class_data[yotogiClassIdFromName].exp_system.SetExreienceList(list2);
						}
						catch (ArgumentException)
						{
							NDebug.Assert("enum parse error.[" + cellAsString2 + "]");
						}
					}
				}
			}
		}
		this.ref_status_ = new MaidParam.StatusAccess(this);
		this.status_.first_name = MaidRandomName.GetFirstName();
		this.status_.last_name = MaidRandomName.GetLastName();
		this.status_.condition = Condition.Tonus;
		this.status_.employment = false;
		this.status_.leader = false;
		this.status_.is_first_name_call = true;
		this.status_.is_rental_maid = false;
		this.status_.current_maid_class = MaidClassType.Novice;
		this.status_.current_yotogi_class = MaidParam.GetYotogiClassIdFromName("Debut");
		GameMain.Instance.CharacterMgr.GetPlayerParam().SetMaidClassOpenFlag(MaidClassType.Novice, true);
		GameMain.Instance.CharacterMgr.GetPlayerParam().SetYotogiClassOpenFlag(MaidParam.GetYotogiClassIdFromName("Debut"), true);
		this.SetMaidPoint(param.Status.kInitMaidPoint + GameMain.Instance.CharacterMgr.GetPlayerParam().status.trophy_bonus_maid_point);
		this.status_.hp = (this.status_.cur_hp = 100);
		this.status_.mind = (this.status_.cur_mind = 100);
		this.status_.reason = (this.status_.cur_reason = 100);
		this.status_.popular_rank = 0;
		this.UpdateFeature();
		this.UpdatePropensity();
		this.InitSkillData();
		this.InitMaidClassTypeAndYotogiClassType();
		this.UpdateProfileComment();
		return true;
	}

	// Token: 0x06001701 RID: 5889 RVA: 0x000A9548 File Offset: 0x000A7748
	public void SetEmployment(bool value)
	{
		if (value && !this.status_.employment)
		{
			this.status_.employment_day = GameMain.Instance.CharacterMgr.GetPlayerParam().status.days;
		}
		this.status_.employment = value;
	}

	// Token: 0x06001702 RID: 5890 RVA: 0x000A959C File Offset: 0x000A779C
	public void SetLeader(bool value)
	{
		this.status_.leader = value;
	}

	// Token: 0x06001703 RID: 5891 RVA: 0x000A95AC File Offset: 0x000A77AC
	public void SetName(string last_name, string first_name)
	{
		this.SetLastName(last_name);
		this.SetFirstName(first_name);
	}

	// Token: 0x06001704 RID: 5892 RVA: 0x000A95BC File Offset: 0x000A77BC
	public void SetLastName(string last_name)
	{
		this.status_.last_name = MaidParam.ConvertString(last_name, 8);
	}

	// Token: 0x06001705 RID: 5893 RVA: 0x000A95D0 File Offset: 0x000A77D0
	public void SetFirstName(string first_name)
	{
		this.status_.first_name = MaidParam.ConvertString(first_name, 8);
	}

	// Token: 0x06001706 RID: 5894 RVA: 0x000A95E4 File Offset: 0x000A77E4
	public static string ConvertString(string str, int max_str_size)
	{
		str = Regex.Replace(str, "[\\r\\n\\t\\\\]", string.Empty);
		if (str.Length == 0)
		{
			str = " ";
		}
		StringInfo stringInfo = new StringInfo(str);
		if (max_str_size < stringInfo.LengthInTextElements)
		{
			str = stringInfo.SubstringByTextElements(0, max_str_size);
		}
		return str;
	}

	// Token: 0x06001707 RID: 5895 RVA: 0x000A9634 File Offset: 0x000A7834
	public void SetMaidPoint(int set_num)
	{
		this.status_.maid_point = wf.RoundMinMax(set_num, 0, 9999);
	}

	// Token: 0x06001708 RID: 5896 RVA: 0x000A9650 File Offset: 0x000A7850
	public void AddMaidPoint(int add_num)
	{
		this.status_.maid_point = this.status_.maid_point + add_num;
		this.status_.maid_point = wf.RoundMinMax(this.status_.maid_point, 0, 9999);
	}

	// Token: 0x06001709 RID: 5897 RVA: 0x000A9694 File Offset: 0x000A7894
	public void SetFreeComment(string comment)
	{
		this.status_.free_comment = MaidParam.ConvertString(comment, 308);
	}

	// Token: 0x0600170A RID: 5898 RVA: 0x000A96AC File Offset: 0x000A78AC
	public bool SetFeature(Feature tareget, bool val)
	{
		if (val)
		{
			return this.status_.feature.Add(tareget);
		}
		return this.status_.feature.Remove(tareget);
	}

	// Token: 0x0600170B RID: 5899 RVA: 0x000A96D8 File Offset: 0x000A78D8
	public bool SetFeature(HashSet<Feature> feature_set)
	{
		this.status_.feature = new HashSet<Feature>(feature_set);
		return true;
	}

	// Token: 0x0600170C RID: 5900 RVA: 0x000A96EC File Offset: 0x000A78EC
	public void ClearFeature()
	{
		this.status_.feature.Clear();
	}

	// Token: 0x0600170D RID: 5901 RVA: 0x000A9700 File Offset: 0x000A7900
	public bool SetPropensity(Propensity tareget, bool val)
	{
		if (val)
		{
			return this.status_.propensity.Add(tareget);
		}
		return this.status_.propensity.Remove(tareget);
	}

	// Token: 0x0600170E RID: 5902 RVA: 0x000A972C File Offset: 0x000A792C
	public bool SetPropensity(HashSet<Propensity> propensity_set)
	{
		this.status_.propensity = new HashSet<Propensity>(propensity_set);
		return true;
	}

	// Token: 0x0600170F RID: 5903 RVA: 0x000A9740 File Offset: 0x000A7940
	public void ClearPropensity()
	{
		this.status_.propensity.Clear();
	}

	// Token: 0x06001710 RID: 5904 RVA: 0x000A9754 File Offset: 0x000A7954
	public void UpdateProfileComment()
	{
		this.UpdateBodyParam();
		if (!this.status_.employment)
		{
			this.UpdateInitPlayNumber();
			this.InitSkillData();
		}
		Func<MaidParam.CsvDataBlock, int, int, bool> success_call_back = delegate(MaidParam.CsvDataBlock block, int x, int y)
		{
			this.status_.profile = this.status_.profile + block.csv.GetCellAsString(block.GetOriginalX(block.max_x - 1), block.GetOriginalY(y));
			return true;
		};
		string text = "profile_comment_1.nei";
		this.status_.profile = string.Empty;
		using (AFileBase afileBase = GameUty.FileSystem.FileOpen(text))
		{
			NDebug.Assert(afileBase.IsValid(), "file open failed.\n" + text);
			using (CsvParser csvParser = new CsvParser())
			{
				if (!csvParser.Open(afileBase))
				{
					NDebug.Assert("csv open failed.\n" + text);
				}
				MaidParam.CsvDataBlock csvDataBlock = new MaidParam.CsvDataBlock(csvParser, 0);
				Func<MaidParam.CsvDataBlock, int, bool> line_func = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					if (!cur_block.csv.IsCellToExistData(cur_block.GetOriginalX(1), line_y))
					{
						return true;
					}
					string cellAsString2 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(1), line_y);
					string cellAsString3 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(2), line_y);
					return !this.status_.parts_dic.ContainsKey(cellAsString2) || this.status_.parts_dic[cellAsString2] == cellAsString3;
				};
				csvDataBlock.BlockAnalysis(1, line_func, success_call_back);
				csvDataBlock.NextBlock();
				Func<MaidParam.CsvDataBlock, int, bool> line_func2 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					if (!cur_block.csv.IsCellToExistData(cur_block.GetOriginalX(1), line_y))
					{
						return true;
					}
					string cellAsString2 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(1), line_y);
					Feature feature = (Feature)((int)Enum.Parse(typeof(Feature), cellAsString2));
					NDebug.Assert(Feature.Null <= feature && feature < Feature.Max, "Range of out. : " + (int)feature);
					return this.status_.feature.Contains(feature);
				};
				csvDataBlock.BlockAnalysis(1, line_func2, success_call_back);
				csvDataBlock.NextBlock();
				csvDataBlock.BlockAnalysis(1, line_func, success_call_back);
			}
		}
		text = "profile_comment_2.nei";
		using (AFileBase afileBase2 = GameUty.FileSystem.FileOpen(text))
		{
			NDebug.Assert(afileBase2.IsValid(), "file open failed.\n" + text);
			using (CsvParser csv = new CsvParser())
			{
				if (!csv.Open(afileBase2))
				{
					NDebug.Assert("csv open failed.\n" + text);
				}
				MaidParam.CsvDataBlock csvDataBlock2 = new MaidParam.CsvDataBlock(csv, 0);
				string cellAsString = csv.GetCellAsString(csvDataBlock2.GetOriginalX(csvDataBlock2.max_x - 1), csvDataBlock2.GetOriginalY(csvDataBlock2.max_y - 1));
				this.status_.profile = this.status_.profile + cellAsString.Replace("[n]", string.Empty + this.status_.body.bust);
				csvDataBlock2.NextBlock();
				cellAsString = csv.GetCellAsString(csvDataBlock2.GetOriginalX(csvDataBlock2.max_x - 1), csvDataBlock2.GetOriginalY(csvDataBlock2.max_y - 1));
				this.status_.profile = this.status_.profile + cellAsString.Replace("[n]", string.Empty + this.status_.body.waist);
				csvDataBlock2.NextBlock();
				cellAsString = csv.GetCellAsString(csvDataBlock2.GetOriginalX(csvDataBlock2.max_x - 1), csvDataBlock2.GetOriginalY(csvDataBlock2.max_y - 1));
				this.status_.profile = this.status_.profile + cellAsString.Replace("[n]", string.Empty + this.status_.body.hip);
				csvDataBlock2.NextBlock();
				Func<MaidParam.CsvDataBlock, int, bool> line_func3 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					string cellAsString2 = csv.GetCellAsString(cur_block.GetOriginalX(1), line_y);
					if (cellAsString2.IndexOf(this.status_.body.cup) == -1)
					{
						return false;
					}
					bool flag = csv.GetCellAsString(cur_block.GetOriginalX(2), line_y) == "○";
					return !flag || 50 <= this.maid_.GetProp(MPN.MuneTare).value;
				};
				csvDataBlock2.BlockAnalysis(1, line_func3, success_call_back);
				csvDataBlock2.NextBlock();
				Func<MaidParam.CsvDataBlock, int, bool> line_func4 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					int cellAsInteger = csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
					if (cellAsInteger <= this.status_.body.waist)
					{
						return false;
					}
					int num = int.MinValue;
					if (csv.IsCellToExistData(cur_block.GetOriginalX(2), line_y))
					{
						num = csv.GetCellAsInteger(cur_block.GetOriginalX(2), line_y);
					}
					int num2 = int.MaxValue;
					if (csv.IsCellToExistData(cur_block.GetOriginalX(3), line_y))
					{
						num2 = csv.GetCellAsInteger(cur_block.GetOriginalX(3), line_y);
					}
					return num <= this.status_.body.hip && this.status_.body.hip <= num2;
				};
				csvDataBlock2.BlockAnalysis(1, line_func4, success_call_back);
				csvDataBlock2.NextBlock();
				Func<MaidParam.CsvDataBlock, int, bool> line_func5 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					Personal cellAsInteger = (Personal)cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
					if (this.status_.personal != cellAsInteger)
					{
						return false;
					}
					if (!cur_block.csv.IsCellToExistData(cur_block.GetOriginalX(2), line_y))
					{
						return true;
					}
					string cellAsString2 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(2), line_y);
					Feature feature = (Feature)((int)Enum.Parse(typeof(Feature), cellAsString2));
					NDebug.Assert(Feature.Null <= feature && feature < Feature.Max, "Range of out. : " + (int)feature);
					return this.status_.feature.Contains(feature);
				};
				csvDataBlock2.BlockAnalysis(1, line_func5, success_call_back);
				csvDataBlock2.NextBlock();
				csvDataBlock2.BlockAnalysis(1, line_func5, success_call_back);
			}
		}
		text = "profile_comment_3.nei";
		using (AFileBase afileBase3 = GameUty.FileSystem.FileOpen(text))
		{
			NDebug.Assert(afileBase3.IsValid(), "file open failed.\n" + text);
			using (CsvParser csv = new CsvParser())
			{
				if (!csv.Open(afileBase3))
				{
					NDebug.Assert("csv open failed.\n" + text);
				}
				MaidParam.CsvDataBlock csvDataBlock3 = new MaidParam.CsvDataBlock(csv, 0);
				Func<MaidParam.CsvDataBlock, int, bool> line_func6 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					int num = int.MinValue;
					if (csv.IsCellToExistData(cur_block.GetOriginalX(1), line_y))
					{
						num = csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
					}
					int num2 = int.MaxValue;
					if (csv.IsCellToExistData(cur_block.GetOriginalX(2), line_y))
					{
						num2 = csv.GetCellAsInteger(cur_block.GetOriginalX(2), line_y);
					}
					if (num > this.status_.body.bust || this.status_.body.bust > num2)
					{
						return false;
					}
					num = int.MinValue;
					if (csv.IsCellToExistData(cur_block.GetOriginalX(3), line_y))
					{
						num = csv.GetCellAsInteger(cur_block.GetOriginalX(3), line_y);
					}
					num2 = int.MaxValue;
					if (csv.IsCellToExistData(cur_block.GetOriginalX(4), line_y))
					{
						num2 = csv.GetCellAsInteger(cur_block.GetOriginalX(4), line_y);
					}
					if (num > this.status_.body.waist || this.status_.body.waist > num2)
					{
						return false;
					}
					num = int.MinValue;
					if (csv.IsCellToExistData(cur_block.GetOriginalX(5), line_y))
					{
						num = csv.GetCellAsInteger(cur_block.GetOriginalX(5), line_y);
					}
					num2 = int.MaxValue;
					if (csv.IsCellToExistData(cur_block.GetOriginalX(6), line_y))
					{
						num2 = csv.GetCellAsInteger(cur_block.GetOriginalX(6), line_y);
					}
					return num <= this.status_.body.hip && this.status_.body.hip <= num2;
				};
				csvDataBlock3.BlockAnalysis(1, line_func6, success_call_back);
				csvDataBlock3.NextBlock();
				Func<MaidParam.CsvDataBlock, int, bool> line_func7 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					Personal cellAsInteger = (Personal)cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
					return this.status_.personal == cellAsInteger;
				};
				csvDataBlock3.BlockAnalysis(1, line_func7, success_call_back);
				csvDataBlock3.NextBlock();
				Func<MaidParam.CsvDataBlock, int, bool> line_func8 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					MaidClassType cellAsInteger = (MaidClassType)cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
					return cellAsInteger == this.status_.current_maid_class;
				};
				csvDataBlock3.BlockAnalysis(1, line_func8, success_call_back);
			}
		}
		text = "profile_comment_4.nei";
		using (AFileBase afileBase4 = GameUty.FileSystem.FileOpen(text))
		{
			NDebug.Assert(afileBase4.IsValid(), "file open failed.\n" + text);
			using (CsvParser csv = new CsvParser())
			{
				if (!csv.Open(afileBase4))
				{
					NDebug.Assert("csv open failed.\n" + text);
				}
				Func<MaidParam.CsvDataBlock, int, bool> line_func9 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					int num = 1;
					if (csv.IsCellToExistData(cur_block.GetOriginalX(num++), line_y))
					{
						int cellAsInteger = cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(num - 1), line_y);
						if (cellAsInteger != this.status_.current_yotogi_class)
						{
							return false;
						}
					}
					int[] array = new int[]
					{
						this.status.play_number,
						this.status.inyoku,
						this.status.lovely,
						this.status.m_value,
						this.status.elegance,
						this.status.hentai,
						this.status.charm,
						this.status.housi
					};
					foreach (int num2 in array)
					{
						if (csv.IsCellToExistData(cur_block.GetOriginalX(num++), line_y))
						{
							int cellAsInteger2 = csv.GetCellAsInteger(cur_block.GetOriginalX(num - 1), line_y);
							if (cellAsInteger2 > num2)
							{
								return false;
							}
						}
					}
					return true;
				};
				MaidParam.CsvDataBlock csvDataBlock4 = new MaidParam.CsvDataBlock(csv, 0);
				Func<MaidParam.CsvDataBlock, int, bool> line_func10 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					Personal cellAsInteger = (Personal)cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
					return this.status_.personal == cellAsInteger;
				};
				csvDataBlock4.BlockAnalysis(1, line_func10, success_call_back);
				csvDataBlock4.NextBlock();
				Func<MaidParam.CsvDataBlock, int, bool> line_func11 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					ContractType cellAsInteger = (ContractType)cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
					return this.status_.contract_type == cellAsInteger;
				};
				csvDataBlock4.BlockAnalysis(1, line_func11, success_call_back);
				csvDataBlock4.NextBlock();
				if (!this.status_.employment && this.status_.seikeiken == Seikeiken.No_No)
				{
					Func<MaidParam.CsvDataBlock, int, bool> line_func12 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
					{
						int num = int.MinValue;
						if (csv.IsCellToExistData(cur_block.GetOriginalX(1), line_y))
						{
							num = cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
						}
						bool flag = num <= this.status.inyoku;
						if (flag)
						{
							this.status_.study_rate = cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(2), line_y);
						}
						return flag;
					};
					csvDataBlock4.BlockAnalysis(1, line_func12, success_call_back);
				}
				csvDataBlock4.NextBlock();
				if (this.status_.employment && this.status_.seikeiken == Seikeiken.No_No)
				{
					csvDataBlock4.BlockAnalysis(1, line_func9, success_call_back);
				}
				csvDataBlock4.NextBlock();
				if (!this.status_.employment && this.status_.seikeiken == Seikeiken.Yes_No)
				{
					Func<MaidParam.CsvDataBlock, int, bool> line_func13 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
					{
						int num = int.MinValue;
						if (csv.IsCellToExistData(cur_block.GetOriginalX(1), line_y))
						{
							num = cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
						}
						bool flag = num <= this.status.play_number;
						if (flag)
						{
							this.status_.study_rate = cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(2), line_y);
						}
						return flag;
					};
					csvDataBlock4.BlockAnalysis(1, line_func13, success_call_back);
				}
				csvDataBlock4.NextBlock();
				if (this.status_.employment && this.status_.seikeiken == Seikeiken.Yes_No)
				{
					csvDataBlock4.BlockAnalysis(1, line_func9, success_call_back);
				}
				csvDataBlock4.NextBlock();
				if (!this.status_.employment && this.status_.seikeiken == Seikeiken.No_Yes)
				{
					Func<MaidParam.CsvDataBlock, int, bool> line_func14 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
					{
						int num = int.MinValue;
						if (csv.IsCellToExistData(cur_block.GetOriginalX(1), line_y))
						{
							num = cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
						}
						bool flag = num <= this.status.play_number;
						if (flag)
						{
							this.status_.study_rate = cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(2), line_y);
						}
						return flag;
					};
					csvDataBlock4.BlockAnalysis(1, line_func14, success_call_back);
				}
				csvDataBlock4.NextBlock();
				if (this.status_.employment && this.status_.seikeiken == Seikeiken.No_Yes)
				{
					csvDataBlock4.BlockAnalysis(1, line_func9, success_call_back);
				}
				csvDataBlock4.NextBlock();
				if (!this.status_.employment && this.status_.seikeiken == Seikeiken.Yes_Yes)
				{
					Func<MaidParam.CsvDataBlock, int, bool> line_func15 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
					{
						int num = int.MinValue;
						if (csv.IsCellToExistData(cur_block.GetOriginalX(1), line_y))
						{
							num = cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(1), line_y);
						}
						bool flag = num <= this.status.play_number;
						if (flag)
						{
							this.status_.study_rate = cur_block.csv.GetCellAsInteger(cur_block.GetOriginalX(2), line_y);
						}
						return flag;
					};
					csvDataBlock4.BlockAnalysis(1, line_func15, success_call_back);
				}
				csvDataBlock4.NextBlock();
				if (this.status_.employment && this.status_.seikeiken == Seikeiken.Yes_Yes)
				{
					csvDataBlock4.BlockAnalysis(1, line_func9, success_call_back);
				}
			}
		}
		text = "profile_comment_5.nei";
		using (AFileBase afileBase5 = GameUty.FileSystem.FileOpen(text))
		{
			NDebug.Assert(afileBase5.IsValid(), "file open failed.\n" + text);
			using (CsvParser csv = new CsvParser())
			{
				if (!csv.Open(afileBase5))
				{
					NDebug.Assert("csv open failed.\n" + text);
				}
				int comment_no = 0;
				MaidParam.CsvDataBlock csvDataBlock5 = new MaidParam.CsvDataBlock(csv, 0);
				Func<MaidParam.CsvDataBlock, int, int, bool> success_call_back2 = delegate(MaidParam.CsvDataBlock succes_block, int x, int y)
				{
					MaidParam <>f__this = this;
					<>f__this.status_.profile = <>f__this.status_.profile + succes_block.csv.GetCellAsString(succes_block.GetOriginalX(succes_block.max_x - 3 + comment_no), succes_block.GetOriginalY(y));
					return true;
				};
				Func<MaidParam.CsvDataBlock, int, bool> line_func16 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					string cellAsString2 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(1), line_y);
					Propensity propensity = (Propensity)((int)Enum.Parse(typeof(Propensity), cellAsString2));
					NDebug.Assert(Propensity.Null <= propensity && propensity < Propensity.Max, "Range of out. : " + (int)propensity);
					if (!this.status_.propensity.Contains(propensity))
					{
						return false;
					}
					string cellAsString3 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(2), line_y);
					string text2 = cellAsString3;
					switch (text2)
					{
					case "処女":
						if (this.status_.seikeiken != Seikeiken.No_No)
						{
							return false;
						}
						break;
					case "前穴":
						if (this.status_.seikeiken != Seikeiken.Yes_No)
						{
							return false;
						}
						break;
					case "後穴":
						if (this.status_.seikeiken != Seikeiken.No_Yes)
						{
							return false;
						}
						break;
					case "両穴":
						if (this.status_.seikeiken != Seikeiken.Yes_Yes)
						{
							return false;
						}
						break;
					}
					int[] array = new int[]
					{
						this.status.inyoku,
						this.status.m_value,
						this.status.hentai,
						this.status.housi
					};
					int num2 = 3;
					foreach (int num3 in array)
					{
						if (csv.IsCellToExistData(cur_block.GetOriginalX(num2++), line_y))
						{
							int cellAsInteger = csv.GetCellAsInteger(cur_block.GetOriginalX(num2 - 1), line_y);
							if (cellAsInteger > num3)
							{
								return false;
							}
						}
					}
					comment_no = 0;
					int cellAsInteger2 = csv.GetCellAsInteger(cur_block.GetOriginalX(num2++), line_y);
					if (0 < cellAsInteger2)
					{
						if (cellAsInteger2 > this.status.lovely && cellAsInteger2 > this.status.elegance && cellAsInteger2 > this.status.charm)
						{
							return false;
						}
						SortedList<int, string> sortedList = new SortedList<int, string>();
						sortedList.Add(this.status.lovely, "可憐");
						if (!sortedList.ContainsKey(this.status.elegance))
						{
							sortedList.Add(this.status.elegance, "気品");
						}
						if (!sortedList.ContainsKey(this.status.charm))
						{
							sortedList.Add(this.status.charm, "魅惑");
						}
						int num4 = sortedList.Count - 1;
						int num5 = 0;
						foreach (KeyValuePair<int, string> keyValuePair in sortedList)
						{
							if (num4 == num5)
							{
								text2 = keyValuePair.Value;
								if (text2 != null)
								{
									if (MaidParam.kYotogiClassIdMax == null)
									{
										MaidParam.kYotogiClassIdMax = new Dictionary<string, int>(2)
										{
											{
												"気品",
												0
											},
											{
												"魅惑",
												1
											}
										};
									}
									int num;
									if (MaidParam.<>f__switchmap1A.TryGetValue(text2, out num))
									{
										if (num != 0)
										{
											if (num == 1)
											{
												comment_no = 2;
											}
										}
										else
										{
											comment_no = 1;
										}
									}
								}
								break;
							}
							num5++;
						}
					}
					return true;
				};
				csvDataBlock5.BlockAnalysis(1, line_func16, success_call_back2);
			}
		}
		text = "profile_comment_6.nei";
		using (AFileBase afileBase6 = GameUty.FileSystem.FileOpen(text))
		{
			NDebug.Assert(afileBase6.IsValid(), "file open failed.\n" + text);
			using (CsvParser csvParser2 = new CsvParser())
			{
				if (!csvParser2.Open(afileBase6))
				{
					NDebug.Assert("csv open failed.\n" + text);
				}
				MaidParam.CsvDataBlock csvDataBlock6 = new MaidParam.CsvDataBlock(csvParser2, 0);
				csvDataBlock6.NextBlock();
				Func<MaidParam.CsvDataBlock, int, bool> line_func17 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
				{
					if (!cur_block.csv.IsCellToExistData(cur_block.GetOriginalX(1), line_y))
					{
						return true;
					}
					string cellAsString2 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(1), line_y);
					Feature feature = (Feature)((int)Enum.Parse(typeof(Feature), cellAsString2));
					NDebug.Assert(Feature.Null <= feature && feature < Feature.Max, "Range of out. : " + (int)feature);
					return this.status_.feature.Contains(feature);
				};
				csvDataBlock6.BlockAnalysis(1, line_func17, success_call_back);
			}
		}
	}

	// Token: 0x06001711 RID: 5905 RVA: 0x000AA174 File Offset: 0x000A8374
	public void UpdateBodyParam()
	{
		Func<MPN, float> func = (MPN mpn) => (float)this.maid_.GetProp(mpn).value;
		float num = 1340f + func(MPN.sintyou) * 4f + func(MPN.DouPer) * (1f + func(MPN.sintyou) * 0.005f) + func(MPN.KubiScl) * 0.5f + func(MPN.HeadY) * 0.5f;
		float num2 = 55f * func(MPN.RegFat) + 50f * func(MPN.sintyou) * 0.5f + 50f * func(MPN.DouPer) * 0.4f;
		float num3 = 55f * func(MPN.RegMeet) + 50f * func(MPN.sintyou) * 0.5f + 50f * func(MPN.DouPer) * 0.4f;
		float num4 = 10f * func(MPN.UdeScl) * 0.1f;
		float num5 = 5f * func(MPN.ArmL) + 5f * func(MPN.sintyou) * 1f + 5f * func(MPN.UdeScl) * 0.5f;
		float num6 = 70f * func(MPN.Hara) + 50f * func(MPN.sintyou) * 0.7f + 50f * func(MPN.west) * func(MPN.Hara) * 0.005f;
		float num7 = 10f * func(MPN.MuneL) * 2f;
		float num8 = num7 * func(MPN.MuneTare) * 0.005f;
		float num9 = 20f * func(MPN.west) * 0.5f + 15f * func(MPN.west) * func(MPN.sintyou) * 0.02f + 15f * func(MPN.west) * func(MPN.DouPer) * 0.01f;
		float num10 = 10f * func(MPN.koshi) + 7f * func(MPN.koshi) * func(MPN.sintyou) * 0.04f;
		float num11 = 4f * func(MPN.kata);
		float num12 = 38000f + num2 + num3 + num4 + num5 + num6 + num7 + num8 + num9 + num10 + num11;
		this.status_.body.height = (int)(num / 10f);
		this.status_.body.weight = (int)(num12 / 1000f);
		float num13 = 70f + func(MPN.MuneL) * 0.31f + func(MPN.west) * 0.02f;
		this.status_.body.bust = (int)num13;
		this.status_.body.waist = (int)(40f + func(MPN.west) * 0.25f + func(MPN.Hara) * 0.35f);
		this.status_.body.hip = (int)(65f + func(MPN.koshi) * 0.3f + func(MPN.RegFat) * 0.025f + func(MPN.RegMeet) * 0.025f);
		if (num13 < 80f)
		{
			this.status_.body.cup = "A";
		}
		else if (110f <= num13)
		{
			this.status_.body.cup = "N";
		}
		else
		{
			char[] array = new char[]
			{
				'B',
				'C',
				'D',
				'E',
				'F',
				'G',
				'H',
				'I',
				'J',
				'K',
				'L',
				'M'
			};
			this.status_.body.cup = array[(int)((num13 - 80f) / 2.5f)].ToString();
		}
	}

	// Token: 0x06001712 RID: 5906 RVA: 0x000AA520 File Offset: 0x000A8720
	public bool IsGetPossiblePropensity(Propensity cehck_propensity)
	{
		if (this.status_.propensity.Contains(cehck_propensity))
		{
			return true;
		}
		HashSet<Propensity> collection = new HashSet<Propensity>(this.status_.propensity);
		this.UpdateFeatureAndPropensity(false, true);
		bool result = this.status_.propensity.Contains(cehck_propensity);
		this.status_.propensity = new HashSet<Propensity>(collection);
		return result;
	}

	// Token: 0x06001713 RID: 5907 RVA: 0x000AA584 File Offset: 0x000A8784
	public List<Feature> UpdateFeature()
	{
		if (!this.status_.employment)
		{
			this.status_.feature.Clear();
		}
		HashSet<Feature> hashSet = new HashSet<Feature>(this.status_.feature);
		this.UpdateFeatureAndPropensity(true, false);
		HashSet<Feature> hashSet2 = new HashSet<Feature>(this.status_.feature);
		foreach (Feature item in hashSet)
		{
			hashSet2.Remove(item);
		}
		List<Feature> list = new List<Feature>();
		foreach (Feature item2 in hashSet2)
		{
			list.Add(item2);
		}
		return list;
	}

	// Token: 0x06001714 RID: 5908 RVA: 0x000AA690 File Offset: 0x000A8890
	public List<Propensity> UpdatePropensity()
	{
		if (!this.status_.employment)
		{
			this.status_.propensity.Clear();
		}
		HashSet<Propensity> hashSet = new HashSet<Propensity>(this.status_.propensity);
		this.UpdateFeatureAndPropensity(false, true);
		HashSet<Propensity> hashSet2 = new HashSet<Propensity>(this.status_.propensity);
		foreach (Propensity item in hashSet)
		{
			hashSet2.Remove(item);
		}
		List<Propensity> list = new List<Propensity>();
		foreach (Propensity item2 in hashSet2)
		{
			list.Add(item2);
		}
		return list;
	}

	// Token: 0x06001715 RID: 5909 RVA: 0x000AA79C File Offset: 0x000A899C
	private void UpdateFeatureAndPropensity(bool update_feature, bool update_propensity)
	{
		Func<MaidParam.CsvDataBlock, int, int, bool> func = delegate(MaidParam.CsvDataBlock block, int x, int y)
		{
			string cellAsString = block.csv.GetCellAsString(block.GetOriginalX(1), block.GetOriginalY(y));
			Feature tareget = (Feature)((int)Enum.Parse(typeof(Feature), cellAsString));
			this.SetFeature(tareget, true);
			return false;
		};
		Func<MaidParam.CsvDataBlock, int, int, bool> func2 = delegate(MaidParam.CsvDataBlock block, int x, int y)
		{
			string cellAsString = block.csv.GetCellAsString(block.GetOriginalX(1), block.GetOriginalY(y));
			Propensity tareget = (Propensity)((int)Enum.Parse(typeof(Propensity), cellAsString));
			this.SetPropensity(tareget, true);
			return false;
		};
		Func<MaidParam.CsvDataBlock, int, bool> line_call_back = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
		{
			string cellAsString = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(2), line_y);
			if (cellAsString != "指定なし")
			{
				string[] array4 = cellAsString.Split(new char[]
				{
					','
				});
				bool flag = false;
				for (int k = 0; k < array4.Length; k++)
				{
					if (array4[k] == "処女")
					{
						if (this.status_.seikeiken == Seikeiken.No_No)
						{
							flag = true;
							break;
						}
					}
					else if (array4[k] == "前穴")
					{
						if (this.status_.seikeiken == Seikeiken.Yes_No)
						{
							flag = true;
							break;
						}
					}
					else if (array4[k] == "後穴")
					{
						if (this.status_.seikeiken == Seikeiken.No_Yes)
						{
							flag = true;
							break;
						}
					}
					else if (array4[k] == "両穴" && this.status_.seikeiken == Seikeiken.Yes_Yes)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			for (int l = cur_block.GetOriginalX(3); l < cur_block.max_x; l += 2)
			{
				if (cur_block.csv.IsCellToExistData(cur_block.GetOriginalX(l), line_y))
				{
					string cellAsString2 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(l), line_y);
					string cellAsString3 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(l + 1), line_y);
					if (int.Parse(cellAsString3) > 0)
					{
						list.Add(new KeyValuePair<string, string>(cellAsString2, cellAsString3));
					}
				}
			}
			return this.IsStatusListCheck(list);
		};
		Func<MaidParam.CsvDataBlock, int, bool> func3 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
		{
			string cellAsString = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(1), line_y);
			Feature feature = (Feature)((int)Enum.Parse(typeof(Feature), cellAsString));
			NDebug.Assert(Feature.Null <= feature && feature < Feature.Max, "Range of out. : " + (int)feature);
			return !this.status_.feature.Contains(feature) && line_call_back(cur_block, line_y);
		};
		Func<MaidParam.CsvDataBlock, int, bool> func4 = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
		{
			string cellAsString = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(1), line_y);
			Propensity propensity = (Propensity)((int)Enum.Parse(typeof(Propensity), cellAsString));
			NDebug.Assert(Propensity.Null <= propensity && propensity < Propensity.Max, "Range of out. : " + (int)propensity);
			return !this.status_.propensity.Contains(propensity) && line_call_back(cur_block, line_y);
		};
		string[] array = new string[]
		{
			"feature_learn.nei",
			"propensity_learn.nei"
		};
		Func<MaidParam.CsvDataBlock, int, bool>[] array2 = new Func<MaidParam.CsvDataBlock, int, bool>[]
		{
			func3,
			func4
		};
		Func<MaidParam.CsvDataBlock, int, int, bool>[] array3 = new Func<MaidParam.CsvDataBlock, int, int, bool>[]
		{
			func,
			func2
		};
		if (!update_feature)
		{
			array[0] = string.Empty;
		}
		if (!update_propensity)
		{
			array[1] = string.Empty;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i] == string.Empty))
			{
				using (AFileBase afileBase = GameUty.FileSystem.FileOpen(array[i]))
				{
					int size = afileBase.GetSize();
					NDebug.Assert(afileBase.IsValid(), "file open failed.\n" + array[i]);
					using (CsvParser csvParser = new CsvParser())
					{
						if (!csvParser.Open(afileBase))
						{
							NDebug.Assert("csv open failed.\n" + array[i]);
						}
						MaidParam.CsvDataBlock csvDataBlock = new MaidParam.CsvDataBlock(csvParser, 1);
						int num = (int)(this.status_.personal + 1);
						if (num != 0)
						{
							for (int j = 0; j < num - 1; j++)
							{
								csvDataBlock.NextBlock();
							}
							csvDataBlock.BlockAnalysis(0, array2[i], array3[i]);
						}
					}
				}
			}
		}
	}

	// Token: 0x06001716 RID: 5910 RVA: 0x000AA99C File Offset: 0x000A8B9C
	public MaidClassType[] GetAcquisitionMaidClassTypeArray()
	{
		string text = "maid_class_acquisition_data.nei";
		List<MaidClassType> result = new List<MaidClassType>();
		Func<MaidParam.CsvDataBlock, int, bool> line_func = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			for (int i = cur_block.GetOriginalX(1); i < cur_block.max_x; i += 2)
			{
				string cellAsString = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(i), line_y);
				if (cellAsString != string.Empty)
				{
					string cellAsString2 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(i + 1), line_y);
					list.Add(new KeyValuePair<string, string>(cellAsString, cellAsString2));
				}
			}
			return this.IsStatusListCheck(list);
		};
		Func<MaidParam.CsvDataBlock, int, int, bool> success_call_back = delegate(MaidParam.CsvDataBlock block, int x, int y)
		{
			string cellAsString = block.csv.GetCellAsString(block.GetOriginalX(0), block.GetOriginalY(y));
			MaidClassType maidClassType = MaidClassType.Novice;
			try
			{
				maidClassType = (MaidClassType)((int)Enum.Parse(typeof(MaidClassType), cellAsString));
			}
			catch (ArgumentException)
			{
				NDebug.Assert("enum parse error.\n param.MaidClassType : " + cellAsString);
			}
			if (MaidParam.EnabledMaidClass(maidClassType))
			{
				result.Add(maidClassType);
			}
			return false;
		};
		using (AFileBase afileBase = GameUty.FileSystem.FileOpen(text))
		{
			NDebug.Assert(afileBase.IsValid(), "file open failed.\n" + text);
			using (CsvParser csvParser = new CsvParser())
			{
				if (!csvParser.Open(afileBase))
				{
					NDebug.Assert("csv open failed.\n" + text);
				}
				MaidParam.CsvDataBlock csvDataBlock = new MaidParam.CsvDataBlock(csvParser, 0);
				csvDataBlock.BlockAnalysis(1, line_func, success_call_back);
			}
		}
		return result.ToArray();
	}

	// Token: 0x06001717 RID: 5911 RVA: 0x000AAAA8 File Offset: 0x000A8CA8
	public int[] GetAcquisitionYotogiClassTypeArray()
	{
		string text = "yotogi_class_acquisition_data.nei";
		List<int> result = new List<int>();
		Func<MaidParam.CsvDataBlock, int, bool> line_func = delegate(MaidParam.CsvDataBlock cur_block, int line_y)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			for (int i = cur_block.GetOriginalX(1); i < cur_block.max_x; i += 2)
			{
				string cellAsString = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(i), line_y);
				if (cellAsString != string.Empty)
				{
					string cellAsString2 = cur_block.csv.GetCellAsString(cur_block.GetOriginalX(i + 1), line_y);
					list.Add(new KeyValuePair<string, string>(cellAsString, cellAsString2));
				}
			}
			return this.IsStatusListCheck(list);
		};
		Func<MaidParam.CsvDataBlock, int, int, bool> success_call_back = delegate(MaidParam.CsvDataBlock block, int x, int y)
		{
			string cellAsString = block.csv.GetCellAsString(block.GetOriginalX(0), block.GetOriginalY(y));
			int yotogiClassIdFromName = MaidParam.GetYotogiClassIdFromName("Debut");
			try
			{
				yotogiClassIdFromName = MaidParam.GetYotogiClassIdFromName(cellAsString);
			}
			catch (ArgumentException)
			{
				NDebug.Assert("enum parse error.\n param.YotogiClassType : " + cellAsString);
			}
			if (MaidParam.EnabledYotogiClass(yotogiClassIdFromName))
			{
				result.Add(yotogiClassIdFromName);
			}
			return false;
		};
		using (AFileBase afileBase = GameUty.FileSystem.FileOpen(text))
		{
			NDebug.Assert(afileBase.IsValid(), "file open failed.\n" + text);
			using (CsvParser csvParser = new CsvParser())
			{
				if (!csvParser.Open(afileBase))
				{
					NDebug.Assert("csv open failed.\n" + text);
				}
				MaidParam.CsvDataBlock csvDataBlock = new MaidParam.CsvDataBlock(csvParser, 0);
				csvDataBlock.BlockAnalysis(1, line_func, success_call_back);
			}
		}
		return result.ToArray();
	}

	// Token: 0x06001718 RID: 5912 RVA: 0x000AABB4 File Offset: 0x000A8DB4
	public MaidClassType[] UpdatetAcquisitionMaidClassType(bool set_have_flag = true)
	{
		List<MaidClassType> list = new List<MaidClassType>();
		PlayerParam playerParam = GameMain.Instance.CharacterMgr.GetPlayerParam();
		MaidClassType[] acquisitionMaidClassTypeArray = this.GetAcquisitionMaidClassTypeArray();
		for (int i = 0; i < acquisitionMaidClassTypeArray.Length; i++)
		{
			param.Status.MaidClassData<MaidClassType> maidClassData = this.status_.maid_class_data[(int)acquisitionMaidClassTypeArray[i]];
			if (!maidClassData.is_have)
			{
				if (set_have_flag)
				{
					maidClassData.is_have = true;
					maidClassData.exp_system.Update();
					playerParam.SetMaidClassOpenFlag(maidClassData.type, true);
				}
				list.Add(acquisitionMaidClassTypeArray[i]);
			}
		}
		return list.ToArray();
	}

	// Token: 0x06001719 RID: 5913 RVA: 0x000AAC4C File Offset: 0x000A8E4C
	public int[] UpdatetAcquisitionYotogiClassType(bool set_have_flag = true)
	{
		List<int> list = new List<int>();
		PlayerParam playerParam = GameMain.Instance.CharacterMgr.GetPlayerParam();
		int[] acquisitionYotogiClassTypeArray = this.GetAcquisitionYotogiClassTypeArray();
		for (int i = 0; i < acquisitionYotogiClassTypeArray.Length; i++)
		{
			param.Status.MaidClassData<int> maidClassData = this.status_.yotogi_class_data[acquisitionYotogiClassTypeArray[i]];
			if (!maidClassData.is_have)
			{
				if (set_have_flag)
				{
					maidClassData.is_have = true;
					maidClassData.exp_system.Update();
					playerParam.SetYotogiClassOpenFlag(maidClassData.type, true);
				}
				list.Add(acquisitionYotogiClassTypeArray[i]);
			}
		}
		return list.ToArray();
	}

	// Token: 0x0600171A RID: 5914 RVA: 0x000AACE4 File Offset: 0x000A8EE4
	private bool IsStatusListCheck(List<KeyValuePair<string, string>> check_list)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		dictionary.Add("経験人数", this.status.play_number);
		dictionary.Add("淫欲", this.status.inyoku);
		dictionary.Add("可憐", this.status.lovely);
		dictionary.Add("M性", this.status.m_value);
		dictionary.Add("気品", this.status.elegance);
		dictionary.Add("変態", this.status.hentai);
		dictionary.Add("魅惑", this.status.charm);
		dictionary.Add("奉仕", this.status.housi);
		for (int i = 0; i < check_list.Count; i++)
		{
			string key = check_list[i].Key;
			int num = int.Parse(check_list[i].Value);
			if (0 <= key.IndexOf(','))
			{
				string[] array = key.Split(new char[]
				{
					','
				});
				bool flag = false;
				for (int j = 0; j < array.Length; j++)
				{
					NDebug.Assert(dictionary.ContainsKey(array[j]), "不明なパラメータ : " + array[j]);
					if (num <= dictionary[array[j]])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			else if (key == "特殊_過去の秘密")
			{
				if (this.status.employment)
				{
					return false;
				}
			}
			else
			{
				if (key == "VIP")
				{
					Dictionary<int, param_player.Status.NightWorkState> night_works_state_dic = GameMain.Instance.CharacterMgr.GetPlayerParam().status.night_works_state_dic;
					return night_works_state_dic.ContainsKey(num) && night_works_state_dic[num].finish;
				}
				if (key == "PFLAG")
				{
					return GameMain.Instance.CharacterMgr.GetPlayerParam().status.GetGenericFlag(check_list[i].Value) != 0;
				}
				NDebug.Assert(dictionary.ContainsKey(key), "不明なパラメータ : " + key);
				if (num > dictionary[key])
				{
					return false;
				}
			}
		}
		return true;
	}

	// Token: 0x0600171B RID: 5915 RVA: 0x000AAF40 File Offset: 0x000A9140
	public void InitMaidClassTypeAndYotogiClassType()
	{
		if (this.status_.employment)
		{
			Debug.LogWarning("初回エディットではないのに呼ばれました");
			return;
		}
		PlayerParam playerParam = GameMain.Instance.CharacterMgr.GetPlayerParam();
		MaidClassType current_maid_class = this.status_.current_maid_class;
		List<MaidClassType> list = new List<MaidClassType>();
		for (int i = 0; i < 32; i++)
		{
			if (this.status_.maid_class_data[i].is_have)
			{
				list.Add((MaidClassType)i);
			}
			this.status_.maid_class_data[i].Clear();
		}
		MaidClassType[] acquisitionMaidClassTypeArray = this.GetAcquisitionMaidClassTypeArray();
		List<MaidClassType> list2 = new List<MaidClassType>();
		foreach (MaidClassType maidClassType in acquisitionMaidClassTypeArray)
		{
			if (playerParam.status.IsMaidClassOpenFlag(maidClassType))
			{
				param.Status.MaidClassData<MaidClassType> maidClassData = this.status_.maid_class_data[(int)maidClassType];
				maidClassData.is_have = true;
				if (!list.Contains(maidClassType))
				{
					list2.Add(maidClassType);
				}
			}
		}
		if (0 < list2.Count)
		{
			this.status_.current_maid_class = list2[list2.Count - 1];
		}
		else if (this.status_.maid_class_data[(int)current_maid_class].is_have)
		{
			this.status_.current_maid_class = current_maid_class;
		}
		else if (0 < acquisitionMaidClassTypeArray.Length)
		{
			this.status_.current_maid_class = acquisitionMaidClassTypeArray[acquisitionMaidClassTypeArray.Length - 1];
		}
		else
		{
			this.status_.current_maid_class = MaidClassType.Novice;
		}
		int current_yotogi_class = this.status_.current_yotogi_class;
		List<int> list3 = new List<int>();
		for (int k = 0; k < MaidParam.kYotogiClassIdMax; k++)
		{
			if (this.status_.yotogi_class_data[k].is_have)
			{
				list3.Add(k);
			}
			this.status_.yotogi_class_data[k].Clear();
		}
		int[] acquisitionYotogiClassTypeArray = this.GetAcquisitionYotogiClassTypeArray();
		List<int> list4 = new List<int>();
		foreach (int num in acquisitionYotogiClassTypeArray)
		{
			if (playerParam.status.IsYotogiClassOpenFlag(num))
			{
				param.Status.MaidClassData<int> maidClassData2 = this.status_.yotogi_class_data[num];
				maidClassData2.is_have = true;
				if (!list3.Contains(num))
				{
					list4.Add(num);
				}
			}
		}
		if (0 < list4.Count)
		{
			this.status_.current_yotogi_class = list4[list4.Count - 1];
		}
		else if (this.status_.yotogi_class_data[current_yotogi_class].is_have)
		{
			this.status_.current_yotogi_class = current_yotogi_class;
		}
		else if (0 < acquisitionYotogiClassTypeArray.Length)
		{
			this.status_.current_yotogi_class = acquisitionYotogiClassTypeArray[acquisitionYotogiClassTypeArray.Length - 1];
		}
		else
		{
			this.status_.current_yotogi_class = MaidParam.GetYotogiClassIdFromName("Debut");
		}
		this.UpdateMaidClassAndYotogiClassStatus();
	}

	// Token: 0x0600171C RID: 5916 RVA: 0x000AB234 File Offset: 0x000A9434
	public void UpdateMaidClassAndYotogiClassStatus()
	{
		this.status_.maid_class_bonus_status.Clear();
		Dictionary<string, Action<int>> status_function_dic = new Dictionary<string, Action<int>>();
		status_function_dic.Add("体力", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.hp = <>f__this.status_.maid_class_bonus_status.hp + add_num;
		});
		status_function_dic.Add("精神力", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.mind = <>f__this.status_.maid_class_bonus_status.mind + add_num;
		});
		status_function_dic.Add("接待", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.reception = <>f__this.status_.maid_class_bonus_status.reception + add_num;
		});
		status_function_dic.Add("お世話", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.care = <>f__this.status_.maid_class_bonus_status.care + add_num;
		});
		status_function_dic.Add("可憐", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.lovely = <>f__this.status_.maid_class_bonus_status.lovely + add_num;
		});
		status_function_dic.Add("淫欲", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.inyoku = <>f__this.status_.maid_class_bonus_status.inyoku + add_num;
		});
		status_function_dic.Add("気品", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.elegance = <>f__this.status_.maid_class_bonus_status.elegance + add_num;
		});
		status_function_dic.Add("M性", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.m_value = <>f__this.status_.maid_class_bonus_status.m_value + add_num;
		});
		status_function_dic.Add("魅惑", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.charm = <>f__this.status_.maid_class_bonus_status.charm + add_num;
		});
		status_function_dic.Add("変態", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.hentai = <>f__this.status_.maid_class_bonus_status.hentai + add_num;
		});
		status_function_dic.Add("奉仕", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.housi = <>f__this.status_.maid_class_bonus_status.housi + add_num;
		});
		status_function_dic.Add("指導", delegate(int add_num)
		{
			MaidParam <>f__this = this;
			<>f__this.status_.maid_class_bonus_status.teach_rate = <>f__this.status_.maid_class_bonus_status.teach_rate + add_num;
		});
		Action<string> action = delegate(string cell_str)
		{
			if (cell_str == string.Empty)
			{
				return;
			}
			string[] array2 = cell_str.Split(new char[]
			{
				','
			});
			foreach (string text in array2)
			{
				if (text.IndexOf('+') != -1)
				{
					string[] array4 = text.Split(new char[]
					{
						'+'
					});
					NDebug.Assert(array4.Length == 2, cell_str + "\ntoken count error : " + array4.Length);
					NDebug.Assert(status_function_dic.ContainsKey(array4[0]), array4[0] + "には未対応です");
					status_function_dic[array4[0]](int.Parse(array4[1]));
				}
				else if (text.IndexOf('-') != -1)
				{
					string[] array5 = text.Split(new char[]
					{
						'-'
					});
					NDebug.Assert(array5.Length == 2, "token count error : " + array5.Length);
					NDebug.Assert(status_function_dic.ContainsKey(array5[0]), array5[0] + "には未対応です");
					status_function_dic[array5[0]](int.Parse(array5[1]) * -1);
				}
				else
				{
					NDebug.Assert("[" + text + "]\n記述が不正です");
				}
			}
		};
		string[] array = new string[]
		{
			"maid_class_bonus_status.nei",
			"yotogi_class_bonus_status.nei"
		};
		for (int i = 0; i < array.Length; i++)
		{
			using (AFileBase afileBase = GameUty.FileSystem.FileOpen(array[i]))
			{
				using (CsvParser csvParser = new CsvParser())
				{
					bool condition = csvParser.Open(afileBase);
					NDebug.Assert(condition, "file open error[" + array + "]");
					for (int j = 1; j < csvParser.max_cell_y; j++)
					{
						if (csvParser.IsCellToExistData(0, j))
						{
							string cellAsString = csvParser.GetCellAsString(0, j);
							try
							{
								int num;
								if (i == 0)
								{
									MaidClassType maidClassType = (MaidClassType)((int)Enum.Parse(typeof(MaidClassType), cellAsString));
									if (!this.status_.maid_class_data[(int)maidClassType].is_have)
									{
										num = 0;
									}
									else
									{
										num = this.status_.maid_class_data[(int)maidClassType].level;
									}
								}
								else
								{
									int yotogiClassIdFromName = MaidParam.GetYotogiClassIdFromName(cellAsString);
									if (!this.status_.yotogi_class_data[yotogiClassIdFromName].is_have)
									{
										num = 0;
									}
									else
									{
										num = this.status_.yotogi_class_data[yotogiClassIdFromName].level;
									}
								}
								for (int k = 1; k <= num; k++)
								{
									if (csvParser.IsCellToExistData(k, j))
									{
										action(csvParser.GetCellAsString(k, j));
									}
								}
							}
							catch (ArgumentException)
							{
								NDebug.Assert("enum parse error.[" + cellAsString + "]");
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x0600171D RID: 5917 RVA: 0x000AB5E8 File Offset: 0x000A97E8
	public void SetSeikeikenFront(bool val)
	{
		if (val)
		{
			if (this.status_.seikeiken == Seikeiken.Yes_Yes || this.status_.seikeiken == Seikeiken.Yes_No)
			{
				return;
			}
			if (this.status_.seikeiken == Seikeiken.No_No)
			{
				this.status_.seikeiken = Seikeiken.Yes_No;
			}
			else
			{
				this.status_.seikeiken = Seikeiken.Yes_Yes;
			}
		}
		else
		{
			if (this.status_.seikeiken == Seikeiken.No_No || this.status_.seikeiken == Seikeiken.No_Yes)
			{
				return;
			}
			if (this.status_.seikeiken == Seikeiken.Yes_Yes)
			{
				this.status_.seikeiken = Seikeiken.No_Yes;
			}
			else
			{
				this.status_.seikeiken = Seikeiken.No_No;
			}
		}
	}

	// Token: 0x0600171E RID: 5918 RVA: 0x000AB6A0 File Offset: 0x000A98A0
	public void SetSeikeikenBack(bool val)
	{
		if (val)
		{
			if (this.status_.seikeiken == Seikeiken.Yes_Yes || this.status_.seikeiken == Seikeiken.No_Yes)
			{
				return;
			}
			if (this.status_.seikeiken == Seikeiken.No_No)
			{
				this.status_.seikeiken = Seikeiken.No_Yes;
			}
			else
			{
				this.status_.seikeiken = Seikeiken.Yes_Yes;
			}
		}
		else
		{
			if (this.status_.seikeiken == Seikeiken.No_No || this.status_.seikeiken == Seikeiken.Yes_No)
			{
				return;
			}
			if (this.status_.seikeiken == Seikeiken.Yes_Yes)
			{
				this.status_.seikeiken = Seikeiken.Yes_No;
			}
			else
			{
				this.status_.seikeiken = Seikeiken.No_No;
			}
		}
	}

	// Token: 0x0600171F RID: 5919 RVA: 0x000AB758 File Offset: 0x000A9958
	public void SetInitSeikeiken(Seikeiken set_initseikeiken)
	{
		this.status_.init_seikeiken = set_initseikeiken;
		if (!this.status_.employment)
		{
			this.UpdateInitPlayNumber();
		}
	}

	// Token: 0x06001720 RID: 5920 RVA: 0x000AB788 File Offset: 0x000A9988
	public void UpdateInitPlayNumber()
	{
		if (this.status_.employment)
		{
			return;
		}
		this.SetPlayNumber(0);
		if (this.status_.init_seikeiken != Seikeiken.No_No)
		{
			using (AFileBase afileBase = GameUty.FileSystem.FileOpen("init_keiken_num_setting.nei"))
			{
				int size = afileBase.GetSize();
				NDebug.Assert(afileBase.IsValid(), "file open failed.\ninit_keiken_num_setting.nei");
				using (CsvParser csvParser = new CsvParser())
				{
					if (!csvParser.Open(afileBase))
					{
						NDebug.Assert("csv open failed.\ninit_keiken_num_setting.nei");
					}
					for (int i = 1; i < csvParser.max_cell_y; i++)
					{
						if (!csvParser.IsCellToExistData(0, i))
						{
							break;
						}
						int num = 0;
						string cellAsString = csvParser.GetCellAsString(num++, i);
						Seikeiken seikeiken = Seikeiken.Yes_No;
						if (cellAsString == "前穴")
						{
							seikeiken = Seikeiken.Yes_No;
						}
						else if (cellAsString == "後穴")
						{
							seikeiken = Seikeiken.No_Yes;
						}
						else if (cellAsString == "両穴")
						{
							seikeiken = Seikeiken.Yes_Yes;
						}
						if (seikeiken == this.status_.init_seikeiken)
						{
							int cellAsInteger = csvParser.GetCellAsInteger(num++, i);
							int cellAsInteger2 = csvParser.GetCellAsInteger(num++, i);
							int cellAsInteger3 = csvParser.GetCellAsInteger(num++, i);
							if (cellAsInteger2 <= this.status_.inyoku && cellAsInteger3 <= this.status_.hentai)
							{
								this.SetPlayNumber(cellAsInteger);
								break;
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06001721 RID: 5921 RVA: 0x000AB958 File Offset: 0x000A9B58
	public void SetSeikeiken(Seikeiken set_seikeiken)
	{
		this.status_.seikeiken = set_seikeiken;
		if (!this.status_.employment)
		{
			this.SetInitSeikeiken(set_seikeiken);
		}
	}

	// Token: 0x06001722 RID: 5922 RVA: 0x000AB980 File Offset: 0x000A9B80
	public void SetPersonal(Personal parsonal)
	{
		if (!MaidParam.IsEnabledPersonal(parsonal))
		{
			return;
		}
		this.status_.personal = parsonal;
	}

	// Token: 0x06001723 RID: 5923 RVA: 0x000AB99C File Offset: 0x000A9B9C
	public void SetContractType(ContractType contract_type)
	{
		this.status_.contract_type = contract_type;
	}

	// Token: 0x06001724 RID: 5924 RVA: 0x000AB9AC File Offset: 0x000A9BAC
	public void SetMaidClassType(MaidClassType set_type)
	{
		this.status_.current_maid_class = set_type;
	}

	// Token: 0x06001725 RID: 5925 RVA: 0x000AB9BC File Offset: 0x000A9BBC
	public void AddMaidClassExp(MaidClassType maid_type, int add_exp)
	{
		this.status_.maid_class_data[(int)maid_type].exp_system.AddExp(add_exp);
	}

	// Token: 0x06001726 RID: 5926 RVA: 0x000AB9D8 File Offset: 0x000A9BD8
	public void AddMaidClassExp(int add_exp)
	{
		this.status_.maid_class_data[(int)this.status_.current_maid_class].exp_system.AddExp(add_exp);
	}

	// Token: 0x06001727 RID: 5927 RVA: 0x000ABA08 File Offset: 0x000A9C08
	public void SetYotogiClassType(int set_type)
	{
		this.status_.current_yotogi_class = set_type;
	}

	// Token: 0x06001728 RID: 5928 RVA: 0x000ABA18 File Offset: 0x000A9C18
	public void AddYotogiClassExp(int yotogi_type, int add_exp)
	{
		this.status_.yotogi_class_data[yotogi_type].exp_system.AddExp(add_exp);
	}

	// Token: 0x06001729 RID: 5929 RVA: 0x000ABA34 File Offset: 0x000A9C34
	public void AddYotogiClassExp(int add_exp)
	{
		this.status_.yotogi_class_data[this.status_.current_yotogi_class].exp_system.AddExp(add_exp);
	}

	// Token: 0x0600172A RID: 5930 RVA: 0x000ABA64 File Offset: 0x000A9C64
	public void SetCondition(Condition condition)
	{
		this.status_.condition = condition;
	}

	// Token: 0x0600172B RID: 5931 RVA: 0x000ABA74 File Offset: 0x000A9C74
	public void SetConditionSpecial(ConditionSpecial special_condition)
	{
		this.status_.condition_special = special_condition;
	}

	// Token: 0x0600172C RID: 5932 RVA: 0x000ABA84 File Offset: 0x000A9C84
	public void SetMarriage(bool marriage)
	{
		this.SetGenericFlag("_maid_param_marriage_flag", (!marriage) ? 0 : 1);
		if (!marriage)
		{
			this.SetNewWife(false);
		}
	}

	// Token: 0x0600172D RID: 5933 RVA: 0x000ABAAC File Offset: 0x000A9CAC
	public void SetNewWife(bool wife)
	{
		this.SetGenericFlag("_maid_param_newwife_flag", (!wife) ? 0 : 1);
		if (wife)
		{
			this.SetMarriage(true);
		}
	}

	// Token: 0x0600172E RID: 5934 RVA: 0x000ABAD4 File Offset: 0x000A9CD4
	public void SetYotogiPlayCount(int set_yotogi_play_count)
	{
		this.status_.yotogi_play_count = wf.NumRound4(set_yotogi_play_count);
	}

	// Token: 0x0600172F RID: 5935 RVA: 0x000ABAE8 File Offset: 0x000A9CE8
	public void AddYotogiPlayCount(int add_yotogi_play_count)
	{
		this.status_.yotogi_play_count = this.status_.yotogi_play_count + add_yotogi_play_count;
		this.status_.yotogi_play_count = wf.NumRound4(this.status_.yotogi_play_count);
	}

	// Token: 0x06001730 RID: 5936 RVA: 0x000ABB24 File Offset: 0x000A9D24
	public void SetOthersPlayCount(int set_others_play_count)
	{
		this.status_.others_play_count = wf.NumRound4(set_others_play_count);
	}

	// Token: 0x06001731 RID: 5937 RVA: 0x000ABB38 File Offset: 0x000A9D38
	public void AddOthersPlayCount(int add_others_play_count)
	{
		this.status_.others_play_count = this.status_.others_play_count + add_others_play_count;
		this.status_.others_play_count = wf.NumRound4(this.status_.others_play_count);
	}

	// Token: 0x06001732 RID: 5938 RVA: 0x000ABB74 File Offset: 0x000A9D74
	public void SetPlayNumber(int set_play_number)
	{
		this.status_.play_number = wf.NumRound4(set_play_number);
	}

	// Token: 0x06001733 RID: 5939 RVA: 0x000ABB88 File Offset: 0x000A9D88
	public void AddPlayNumber(int add_play_number)
	{
		this.status_.play_number = this.status_.play_number + add_play_number;
		this.status_.play_number = wf.NumRound4(this.status_.play_number);
	}

	// Token: 0x06001734 RID: 5940 RVA: 0x000ABBC4 File Offset: 0x000A9DC4
	public void SetLikability(int set_likability)
	{
		this.status_.likability = wf.NumRound3(set_likability);
	}

	// Token: 0x06001735 RID: 5941 RVA: 0x000ABBD8 File Offset: 0x000A9DD8
	public void AddLikability(int add_likability)
	{
		this.status_.likability = this.status_.likability + add_likability;
		this.status_.likability = wf.NumRound3(this.status_.likability);
	}

	// Token: 0x06001736 RID: 5942 RVA: 0x000ABC14 File Offset: 0x000A9E14
	public void SetCurExcite(int set_val)
	{
		this.status_.cur_excite = wf.RoundMinMax(set_val, -100, 300);
	}

	// Token: 0x06001737 RID: 5943 RVA: 0x000ABC30 File Offset: 0x000A9E30
	public void AddCurExcite(int add_val)
	{
		this.status_.cur_excite = this.status_.cur_excite + add_val;
		this.status_.cur_excite = wf.RoundMinMax(this.status_.cur_excite, -100, 300);
	}

	// Token: 0x06001738 RID: 5944 RVA: 0x000ABC68 File Offset: 0x000A9E68
	public void SetHp(int set_hp)
	{
		this.status_.hp = wf.NumRound3(set_hp);
	}

	// Token: 0x06001739 RID: 5945 RVA: 0x000ABC7C File Offset: 0x000A9E7C
	public void AddHp(int add_hp)
	{
		this.status_.hp = this.status_.hp + add_hp;
		this.status_.hp = wf.NumRound3(this.status_.hp);
	}

	// Token: 0x0600173A RID: 5946 RVA: 0x000ABCB8 File Offset: 0x000A9EB8
	public void SetCurHp(int set_cur_hp)
	{
		this.status_.cur_hp = wf.RoundMinMax(set_cur_hp, 0, this.status.hp);
	}

	// Token: 0x0600173B RID: 5947 RVA: 0x000ABCE4 File Offset: 0x000A9EE4
	public void SetMind(int set_mind)
	{
		this.status_.mind = wf.NumRound3(set_mind);
	}

	// Token: 0x0600173C RID: 5948 RVA: 0x000ABCF8 File Offset: 0x000A9EF8
	public void AddMind(int add_mind)
	{
		this.status_.mind = this.status_.mind + add_mind;
		this.status_.mind = wf.NumRound3(this.status_.mind);
	}

	// Token: 0x0600173D RID: 5949 RVA: 0x000ABD34 File Offset: 0x000A9F34
	public void SetCurMind(int set_cur_mind)
	{
		this.status_.cur_mind = wf.RoundMinMax(set_cur_mind, 0, this.status.mind);
	}

	// Token: 0x0600173E RID: 5950 RVA: 0x000ABD60 File Offset: 0x000A9F60
	public void AddCurMind(int add_cur_mind)
	{
		this.status_.cur_mind = this.status_.cur_mind + add_cur_mind;
		this.status_.cur_mind = wf.RoundMinMax(this.status_.cur_mind, 0, this.status.mind);
	}

	// Token: 0x0600173F RID: 5951 RVA: 0x000ABDA8 File Offset: 0x000A9FA8
	public void SetReason(int set_reason)
	{
		this.status_.reason = wf.NumRound3(set_reason);
	}

	// Token: 0x06001740 RID: 5952 RVA: 0x000ABDBC File Offset: 0x000A9FBC
	public void AddReason(int add_reason)
	{
		this.status_.reason = this.status_.reason + add_reason;
		this.status_.reason = wf.NumRound3(this.status_.reason);
	}

	// Token: 0x06001741 RID: 5953 RVA: 0x000ABDF8 File Offset: 0x000A9FF8
	public void SetCurReason(int set_cur_reason)
	{
		this.status_.cur_reason = wf.RoundMinMax(set_cur_reason, 0, this.status.reason);
	}

	// Token: 0x06001742 RID: 5954 RVA: 0x000ABE24 File Offset: 0x000AA024
	public void AddCurReason(int add_cur_reason)
	{
		this.status_.cur_reason = this.status_.cur_reason + add_cur_reason;
		this.status_.cur_reason = wf.RoundMinMax(this.status_.cur_reason, 0, this.status.reason);
	}

	// Token: 0x06001743 RID: 5955 RVA: 0x000ABE6C File Offset: 0x000AA06C
	public void SetReception(int set_reception)
	{
		this.status_.reception = wf.NumRound4(set_reception);
	}

	// Token: 0x06001744 RID: 5956 RVA: 0x000ABE80 File Offset: 0x000AA080
	public void AddReception(int add_reception)
	{
		this.status_.reception = this.status_.reception + add_reception;
		this.status_.reception = wf.NumRound4(this.status_.reception);
	}

	// Token: 0x06001745 RID: 5957 RVA: 0x000ABEBC File Offset: 0x000AA0BC
	public void SetCare(int set_care)
	{
		this.status_.care = wf.NumRound4(set_care);
	}

	// Token: 0x06001746 RID: 5958 RVA: 0x000ABED0 File Offset: 0x000AA0D0
	public void AddCare(int add_care)
	{
		this.status_.care = this.status_.care + add_care;
		this.status_.care = wf.NumRound4(this.status_.care);
	}

	// Token: 0x06001747 RID: 5959 RVA: 0x000ABF0C File Offset: 0x000AA10C
	public void SetLovely(int set_lovely)
	{
		this.status_.lovely = wf.NumRound4(set_lovely);
	}

	// Token: 0x06001748 RID: 5960 RVA: 0x000ABF20 File Offset: 0x000AA120
	public void AddLovely(int add_lovely)
	{
		this.status_.lovely = this.status_.lovely + add_lovely;
		this.status_.lovely = wf.NumRound4(this.status_.lovely);
	}

	// Token: 0x06001749 RID: 5961 RVA: 0x000ABF5C File Offset: 0x000AA15C
	public void SetInyoku(int set_inyoku)
	{
		this.status_.inyoku = wf.NumRound4(set_inyoku);
	}

	// Token: 0x0600174A RID: 5962 RVA: 0x000ABF70 File Offset: 0x000AA170
	public void AddInyoku(int add_inyoku)
	{
		this.status_.inyoku = this.status_.inyoku + add_inyoku;
		this.status_.inyoku = wf.NumRound4(this.status_.inyoku);
	}

	// Token: 0x0600174B RID: 5963 RVA: 0x000ABFAC File Offset: 0x000AA1AC
	public void SetElegance(int set_elegance)
	{
		this.status_.elegance = wf.NumRound4(set_elegance);
	}

	// Token: 0x0600174C RID: 5964 RVA: 0x000ABFC0 File Offset: 0x000AA1C0
	public void AddElegance(int add_elegance)
	{
		this.status_.elegance = this.status_.elegance + add_elegance;
		this.status_.elegance = wf.NumRound4(this.status_.elegance);
	}

	// Token: 0x0600174D RID: 5965 RVA: 0x000ABFFC File Offset: 0x000AA1FC
	public void SetMValue(int set_m_value)
	{
		this.status_.m_value = wf.NumRound4(set_m_value);
	}

	// Token: 0x0600174E RID: 5966 RVA: 0x000AC010 File Offset: 0x000AA210
	public void AddMValue(int add_m_value)
	{
		this.status_.m_value = this.status_.m_value + add_m_value;
		this.status_.m_value = wf.NumRound4(this.status_.m_value);
	}

	// Token: 0x0600174F RID: 5967 RVA: 0x000AC04C File Offset: 0x000AA24C
	public void SetCharm(int set_charm)
	{
		this.status_.charm = wf.NumRound4(set_charm);
	}

	// Token: 0x06001750 RID: 5968 RVA: 0x000AC060 File Offset: 0x000AA260
	public void AddCharm(int add_charm)
	{
		this.status_.charm = this.status_.charm + add_charm;
		this.status_.charm = wf.NumRound4(this.status_.charm);
	}

	// Token: 0x06001751 RID: 5969 RVA: 0x000AC09C File Offset: 0x000AA29C
	public void SetHentai(int set_hentai)
	{
		this.status_.hentai = wf.NumRound4(set_hentai);
	}

	// Token: 0x06001752 RID: 5970 RVA: 0x000AC0B0 File Offset: 0x000AA2B0
	public void AddHentai(int add_hentai)
	{
		this.status_.hentai = this.status_.hentai + add_hentai;
		this.status_.hentai = wf.NumRound4(this.status_.hentai);
	}

	// Token: 0x06001753 RID: 5971 RVA: 0x000AC0EC File Offset: 0x000AA2EC
	public void SetHousi(int set_housi)
	{
		this.status_.housi = wf.NumRound4(set_housi);
	}

	// Token: 0x06001754 RID: 5972 RVA: 0x000AC100 File Offset: 0x000AA300
	public void AddHousi(int add_housi)
	{
		this.status_.housi = this.status_.housi + add_housi;
		this.status_.housi = wf.NumRound4(this.status_.housi);
	}

	// Token: 0x06001755 RID: 5973 RVA: 0x000AC13C File Offset: 0x000AA33C
	public void SetFrustration(int setfrustration)
	{
		this.status_.frustration = wf.RoundMinMax(setfrustration, 0, 100);
	}

	// Token: 0x06001756 RID: 5974 RVA: 0x000AC154 File Offset: 0x000AA354
	public void AddFrustration(int add_frustration)
	{
		this.status_.frustration = this.status_.frustration + add_frustration;
		this.status_.frustration = wf.RoundMinMax(this.status_.frustration, 0, 100);
	}

	// Token: 0x06001757 RID: 5975 RVA: 0x000AC188 File Offset: 0x000AA388
	public void SetPopularRank(int set_rank)
	{
		this.status_.popular_rank = wf.RoundMinMax(set_rank, 0, 99);
	}

	// Token: 0x06001758 RID: 5976 RVA: 0x000AC1A0 File Offset: 0x000AA3A0
	public void SetNoonWorkId(int work_id)
	{
		this.status_.noon_work_id = work_id;
	}

	// Token: 0x06001759 RID: 5977 RVA: 0x000AC1B0 File Offset: 0x000AA3B0
	public void SetNightWorkId(int work_id)
	{
		this.status_.night_work_id = work_id;
	}

	// Token: 0x0600175A RID: 5978 RVA: 0x000AC1C0 File Offset: 0x000AA3C0
	public void SetEvaluation(long set_evaluation)
	{
		this.status_.evaluation = set_evaluation;
		this.status_.evaluation = wf.NumRound6(this.status_.evaluation);
	}

	// Token: 0x0600175B RID: 5979 RVA: 0x000AC1EC File Offset: 0x000AA3EC
	public void AddEvaluation(long add_evaluation)
	{
		if (GameMain.Instance.CharacterMgr.GetPlayerParam().IsAvailableRanking())
		{
			this.status_.evaluation = this.status_.evaluation + add_evaluation;
			this.status_.evaluation = wf.NumRound6(this.status_.evaluation);
		}
		this.AddTotalEvaluation(add_evaluation);
	}

	// Token: 0x0600175C RID: 5980 RVA: 0x000AC248 File Offset: 0x000AA448
	public void SetTotalEvaluation(long set_total_evaluation)
	{
		this.status_.total_evaluation = set_total_evaluation;
		this.status_.total_evaluation = wf.NumRound6(this.status_.total_evaluation);
	}

	// Token: 0x0600175D RID: 5981 RVA: 0x000AC274 File Offset: 0x000AA474
	public void AddTotalEvaluation(long add_total_evaluation)
	{
		this.status_.total_evaluation = this.status_.total_evaluation + add_total_evaluation;
		this.status_.total_evaluation = wf.NumRound6(this.status_.total_evaluation);
	}

	// Token: 0x0600175E RID: 5982 RVA: 0x000AC2B0 File Offset: 0x000AA4B0
	public void SetSales(long set_sales)
	{
		this.status_.sales = set_sales;
		this.status_.sales = wf.RoundMinMax(this.status_.sales, 0L, param_player.Status.kmoneyMax);
	}

	// Token: 0x0600175F RID: 5983 RVA: 0x000AC2EC File Offset: 0x000AA4EC
	public void AddSales(long add_sales)
	{
		if (GameMain.Instance.CharacterMgr.GetPlayerParam().IsAvailableRanking())
		{
			this.status_.sales = this.status_.sales + add_sales;
			this.status_.sales = wf.RoundMinMax(this.status_.sales, 0L, param_player.Status.kmoneyMax);
		}
		this.AddTotalSales(add_sales);
	}

	// Token: 0x06001760 RID: 5984 RVA: 0x000AC350 File Offset: 0x000AA550
	public void SetTotalSales(long set_total_sales)
	{
		this.status_.total_sales = set_total_sales;
		this.status_.total_sales = wf.RoundMinMax(this.status_.total_sales, 0L, param_player.Status.kmoneyMax);
	}

	// Token: 0x06001761 RID: 5985 RVA: 0x000AC38C File Offset: 0x000AA58C
	public void AddTotalSales(long add_total_sales)
	{
		this.status_.total_sales = this.status_.total_sales + add_total_sales;
		this.status_.total_sales = wf.RoundMinMax(this.status_.total_sales, 0L, param_player.Status.kmoneyMax);
	}

	// Token: 0x06001762 RID: 5986 RVA: 0x000AC3C4 File Offset: 0x000AA5C4
	public void SetFirstNameCall(bool value)
	{
		this.status_.is_first_name_call = value;
	}

	// Token: 0x06001763 RID: 5987 RVA: 0x000AC3D4 File Offset: 0x000AA5D4
	public void SetRentalMaid(bool value)
	{
		this.status_.is_rental_maid = value;
	}

	// Token: 0x06001764 RID: 5988 RVA: 0x000AC3E4 File Offset: 0x000AA5E4
	public void SetStudyRate(int set_study_rate)
	{
		this.status_.study_rate = wf.RoundMinMax(set_study_rate, 0, 1000);
	}

	// Token: 0x06001765 RID: 5989 RVA: 0x000AC400 File Offset: 0x000AA600
	public void AddStudyRate(int add_study_rate)
	{
		this.status_.study_rate = this.status_.study_rate + add_study_rate;
		this.status_.study_rate = wf.RoundMinMax(this.status_.study_rate, 0, 1000);
	}

	// Token: 0x06001766 RID: 5990 RVA: 0x000AC444 File Offset: 0x000AA644
	public void SetTeachRate(int set_teach_rate)
	{
		this.status_.teach_rate = wf.RoundMinMax(set_teach_rate, 0, 1000);
	}

	// Token: 0x06001767 RID: 5991 RVA: 0x000AC460 File Offset: 0x000AA660
	public void AddTeachRate(int add_teach_rate)
	{
		this.status_.teach_rate = this.status_.teach_rate + add_teach_rate;
		wf.RoundMinMax(this.status_.teach_rate, 0, 1000);
	}

	// Token: 0x06001768 RID: 5992 RVA: 0x000AC498 File Offset: 0x000AA698
	public void SetSexual(param.Status.Sexual sexual)
	{
		this.status_.sexual = sexual;
	}

	// Token: 0x06001769 RID: 5993 RVA: 0x000AC4A8 File Offset: 0x000AA6A8
	public void SetSexualMouth(int set_val)
	{
		this.status_.sexual.mouth = wf.RoundMinMax(set_val, 0, 1000);
	}

	// Token: 0x0600176A RID: 5994 RVA: 0x000AC4C8 File Offset: 0x000AA6C8
	public void AddSexualMouth(int add_val, int limit = 1000)
	{
		this.status_.sexual.mouth = this.status_.sexual.mouth + add_val;
		this.status_.sexual.mouth = wf.RoundMinMax(this.status_.sexual.mouth, 0, limit);
	}

	// Token: 0x0600176B RID: 5995 RVA: 0x000AC514 File Offset: 0x000AA714
	public void SetSexualThroat(int set_val)
	{
		this.status_.sexual.throat = wf.RoundMinMax(set_val, 0, 1000);
	}

	// Token: 0x0600176C RID: 5996 RVA: 0x000AC534 File Offset: 0x000AA734
	public void AddSexualThroat(int add_val, int limit = 1000)
	{
		this.status_.sexual.throat = this.status_.sexual.throat + add_val;
		this.status_.sexual.throat = wf.RoundMinMax(this.status_.sexual.throat, 0, limit);
	}

	// Token: 0x0600176D RID: 5997 RVA: 0x000AC580 File Offset: 0x000AA780
	public void SetSexualNipple(int set_val)
	{
		this.status_.sexual.nipple = wf.RoundMinMax(set_val, 0, 1000);
	}

	// Token: 0x0600176E RID: 5998 RVA: 0x000AC5A0 File Offset: 0x000AA7A0
	public void AddSexualNipple(int add_val, int limit = 1000)
	{
		this.status_.sexual.nipple = this.status_.sexual.nipple + add_val;
		this.status_.sexual.nipple = wf.RoundMinMax(this.status_.sexual.nipple, 0, limit);
	}

	// Token: 0x0600176F RID: 5999 RVA: 0x000AC5EC File Offset: 0x000AA7EC
	public void SetSexualFront(int set_val)
	{
		this.status_.sexual.front = wf.RoundMinMax(set_val, 0, 1000);
		this.status_.sexual.front = 0;
	}

	// Token: 0x06001770 RID: 6000 RVA: 0x000AC61C File Offset: 0x000AA81C
	public void AddSexualFront(int add_val, int limit = 1000)
	{
		this.status_.sexual.front = this.status_.sexual.front + add_val;
		this.status_.sexual.front = wf.RoundMinMax(this.status_.sexual.front, 0, limit);
		this.status_.sexual.front = 0;
	}

	// Token: 0x06001771 RID: 6001 RVA: 0x000AC67C File Offset: 0x000AA87C
	public void SetSexualBack(int set_val)
	{
		this.status_.sexual.back = wf.RoundMinMax(set_val, 0, 1000);
		this.status_.sexual.back = 0;
	}

	// Token: 0x06001772 RID: 6002 RVA: 0x000AC6AC File Offset: 0x000AA8AC
	public void AddSexualBack(int add_val, int limit = 1000)
	{
		this.status_.sexual.back = this.status_.sexual.back + add_val;
		this.status_.sexual.back = wf.RoundMinMax(this.status_.sexual.back, 0, limit);
		this.status_.sexual.back = 0;
	}

	// Token: 0x06001773 RID: 6003 RVA: 0x000AC70C File Offset: 0x000AA90C
	public void SetSexualCuri(int set_val)
	{
		this.status_.sexual.curi = wf.RoundMinMax(set_val, 0, 1000);
	}

	// Token: 0x06001774 RID: 6004 RVA: 0x000AC72C File Offset: 0x000AA92C
	public void AddSexualCuri(int add_val, int limit = 1000)
	{
		this.status_.sexual.curi = this.status_.sexual.curi + add_val;
		this.status_.sexual.curi = wf.RoundMinMax(this.status_.sexual.curi, 0, limit);
	}

	// Token: 0x06001775 RID: 6005 RVA: 0x000AC778 File Offset: 0x000AA978
	public bool SetNewGetSkill(int skill_id)
	{
		if (this.status.IsGetSkill(skill_id))
		{
			return false;
		}
		Yotogi.SkillData skillData = Yotogi.GetSkillData(skill_id);
		param.Status.SkillData skillData2 = new param.Status.SkillData();
		skillData2.id = skill_id;
		skillData2.exp_system.SetExreienceList(new List<int>(skillData.skill_exp_table));
		this.status_.skill_data.Add(skill_id, skillData2);
		return true;
	}

	// Token: 0x06001776 RID: 6006 RVA: 0x000AC7D8 File Offset: 0x000AA9D8
	public bool RemoveSkill(int skill_id)
	{
		if (!this.status.IsGetSkill(skill_id))
		{
			return false;
		}
		this.status_.skill_data.Remove(skill_id);
		return true;
	}

	// Token: 0x06001777 RID: 6007 RVA: 0x000AC80C File Offset: 0x000AAA0C
	public bool AddSkillExp(int skill_id, int add_val)
	{
		NDebug.Assert(this.status.IsGetSkill(skill_id), "未取得のスキルID[" + skill_id.ToString() + "]のデータを参照しようとしました");
		param.Status.SkillData skillData = this.status_.skill_data[skill_id];
		int level = skillData.level;
		skillData.exp_system.AddExp(add_val);
		return level != skillData.level;
	}

	// Token: 0x06001778 RID: 6008 RVA: 0x000AC874 File Offset: 0x000AAA74
	public void AddSkillPlayCount(int skill_id)
	{
		NDebug.Assert(this.status.IsGetSkill(skill_id), "未取得のスキルID[" + skill_id.ToString() + "]のデータを参照しようとしました");
		param.Status.SkillData skillData = this.status_.skill_data[skill_id];
		skillData.play_count += 1u;
	}

	// Token: 0x06001779 RID: 6009 RVA: 0x000AC8C8 File Offset: 0x000AAAC8
	public bool SetNewGetWork(int work_id)
	{
		if (this.status.IsGetWork(work_id))
		{
			return false;
		}
		param.Status.WorkData workData = new param.Status.WorkData();
		workData.id = work_id;
		workData.level = 1;
		this.status_.work_data.Add(work_id, workData);
		return true;
	}

	// Token: 0x0600177A RID: 6010 RVA: 0x000AC910 File Offset: 0x000AAB10
	public bool RemoveWork(int work_id)
	{
		if (!this.status.IsGetWork(work_id))
		{
			return false;
		}
		this.status_.work_data.Remove(work_id);
		return true;
	}

	// Token: 0x0600177B RID: 6011 RVA: 0x000AC944 File Offset: 0x000AAB44
	public void SetWorkLevel(int work_id, int level)
	{
		NDebug.Assert(this.status.IsGetWork(work_id), "未取得の仕事ID[" + work_id.ToString() + "]のデータを参照しようとしました");
		param.Status.WorkData workData = this.status_.work_data[work_id];
		workData.level = level;
	}

	// Token: 0x0600177C RID: 6012 RVA: 0x000AC994 File Offset: 0x000AAB94
	public void AddWorkPlayCount(int work_id)
	{
		NDebug.Assert(this.status.IsGetWork(work_id), "未取得の仕事ID[" + work_id.ToString() + "]のデータを参照しようとしました");
		param.Status.WorkData workData = this.status_.work_data[work_id];
		workData.play_count += 1u;
	}

	// Token: 0x0600177D RID: 6013 RVA: 0x000AC9E8 File Offset: 0x000AABE8
	public void SetEyePartsTab(EyePartsTab set_tab)
	{
		this.status_.eye_parts_tab = set_tab;
	}

	// Token: 0x0600177E RID: 6014 RVA: 0x000AC9F8 File Offset: 0x000AABF8
	public void SetPartsValue(string parts_key, string val)
	{
		if (this.status_.parts_dic.ContainsKey(parts_key))
		{
			this.status_.parts_dic[parts_key] = val;
		}
		else
		{
			this.status_.parts_dic.Add(parts_key, val);
		}
	}

	// Token: 0x0600177F RID: 6015 RVA: 0x000ACA44 File Offset: 0x000AAC44
	public void SetGenericFlag(string flag_name, int val)
	{
		if (this.status_.generic_flag.ContainsKey(flag_name))
		{
			this.status_.generic_flag[flag_name] = val;
		}
		else
		{
			this.status_.generic_flag.Add(flag_name, val);
		}
		if (0 < val)
		{
			FreeModeItemEveryday.CheckEveryDayFlag(flag_name);
		}
	}

	// Token: 0x06001780 RID: 6016 RVA: 0x000ACAA0 File Offset: 0x000AACA0
	public void SetGenericFlag(Dictionary<string, int> dic)
	{
		this.status_.generic_flag = new Dictionary<string, int>(dic);
	}

	// Token: 0x17000305 RID: 773
	// (get) Token: 0x06001781 RID: 6017 RVA: 0x000ACAB4 File Offset: 0x000AACB4
	public MaidParam.StatusAccess status
	{
		get
		{
			return this.ref_status_;
		}
	}

	// Token: 0x06001782 RID: 6018 RVA: 0x000ACABC File Offset: 0x000AACBC
	public void Serialize(BinaryWriter binary)
	{
		binary.Write("CM3D2_MAID_PPARAM");
		binary.Write(155);
		this.status_.Serialize(binary);
		binary.Write(1923480616);
	}

	// Token: 0x06001783 RID: 6019 RVA: 0x000ACAF8 File Offset: 0x000AACF8
	public void Deserialize(BinaryReader binary)
	{
		string a = binary.ReadString();
		NDebug.Assert(a == "CM3D2_MAID_PPARAM", "メイドパラメータのヘッダーが不正です。");
		int version = binary.ReadInt32();
		this.status_.Deserialize(binary, version);
		for (int i = 0; i < 7; i++)
		{
			MaidClassType check_maid_class = (MaidClassType)i;
			if (!MaidParam.EnabledMaidClass(check_maid_class))
			{
				this.status_.maid_class_data[i].is_have = false;
			}
		}
		if (!MaidParam.EnabledMaidClass(this.status.current_maid_class))
		{
			this.SetMaidClassType(MaidClassType.Novice);
		}
		for (int j = 0; j < MaidParam.kYotogiClassIdEnabledMax; j++)
		{
			int check_yotogi_class = j;
			if (!MaidParam.EnabledYotogiClass(check_yotogi_class))
			{
				this.status_.yotogi_class_data[j].is_have = false;
			}
		}
		if (!MaidParam.EnabledYotogiClass(this.status.current_yotogi_class))
		{
			this.SetYotogiClassType(MaidParam.GetYotogiClassIdFromName("Debut"));
		}
		this.UpdateMaidClassAndYotogiClassStatus();
		int num = binary.ReadInt32();
		NDebug.Assert(1923480616 == num, "メイドパラメータのロードに失敗しました");
		if (!MaidParam.IsEnabledPersonal(this.status.personal))
		{
			this.SetPersonal(Personal.Pure);
		}
	}

	// Token: 0x06001784 RID: 6020 RVA: 0x000ACC24 File Offset: 0x000AAE24
	private bool AddMaidParamStatus(ref int ref_target_point, int add_num)
	{
		if (0 < add_num)
		{
			if (this.status_.maid_point <= 0)
			{
				return false;
			}
			if (this.status_.maid_point - add_num < 0)
			{
				add_num = this.status_.maid_point;
			}
		}
		else if (add_num < 0)
		{
			if (ref_target_point <= 0)
			{
				return false;
			}
			if (ref_target_point + add_num < 0)
			{
				add_num = ref_target_point * -1;
			}
		}
		ref_target_point += add_num;
		this.status_.maid_point = this.status_.maid_point - add_num;
		return add_num != 0;
	}

	// Token: 0x06001785 RID: 6021 RVA: 0x000ACCB4 File Offset: 0x000AAEB4
	private void InitSkillData()
	{
		this.status_.skill_data.Clear();
		SortedDictionary<int, Yotogi.SkillData>[] skill_data_list = Yotogi.skill_data_list;
		for (int i = 0; i < skill_data_list.Length; i++)
		{
			foreach (KeyValuePair<int, Yotogi.SkillData> keyValuePair in skill_data_list[i])
			{
				if (keyValuePair.Value.IsCheckGetSkill(this.maid_, false, false))
				{
					this.SetNewGetSkill(keyValuePair.Key);
				}
			}
		}
	}

	// Token: 0x0400149D RID: 5277
	private const int kSaveDataCheckCode = 1923480616;

	// Token: 0x0400149E RID: 5278
	private const string kFileNameProfileComment = "profile_comment";

	// Token: 0x0400149F RID: 5279
	private const string kCsvFileNameFeatureLearn = "feature_learn.nei";

	// Token: 0x040014A0 RID: 5280
	private const string kCsvFileNamePropensityLearn = "propensity_learn.nei";

	// Token: 0x040014A1 RID: 5281
	private static Dictionary<MaidClassType, string> maid_class_enabled_ = new Dictionary<MaidClassType, string>();

	// Token: 0x040014A2 RID: 5282
	private static Dictionary<int, string> yotogi_class_enabled_ = new Dictionary<int, string>();

	// Token: 0x040014A3 RID: 5283
	private static Dictionary<string, int> yotogi_class_name_map_ = new Dictionary<string, int>();

	// Token: 0x040014A4 RID: 5284
	private static Dictionary<int, string> yotogi_class_id_map_ = new Dictionary<int, string>();

	// Token: 0x040014A5 RID: 5285
	private MaidParam.StatusAccess ref_status_;

	// Token: 0x040014A6 RID: 5286
	private Maid maid_;

	// Token: 0x040014A7 RID: 5287
	private param.Status status_;

	// Token: 0x020002E9 RID: 745
	public class StatusAccess
	{
		// Token: 0x0600178D RID: 6029 RVA: 0x000AD174 File Offset: 0x000AB374
		public StatusAccess(MaidParam par)
		{
			this.par_ = par;
			this.maid_class_data_access_ = new MaidParam.StatusAccess.MaidClassDataAccess<MaidClassType>[this.par_.status_.maid_class_data.Length];
			for (int i = 0; i < this.maid_class_data_access_.Length; i++)
			{
				this.maid_class_data_access_[i] = new MaidParam.StatusAccess.MaidClassDataAccess<MaidClassType>(this.par_.status_.maid_class_data[i]);
			}
			this.yotogi_class_data_access_ = new MaidParam.StatusAccess.MaidClassDataAccess<int>[this.par_.status_.yotogi_class_data.Length];
			for (int j = 0; j < this.yotogi_class_data_access_.Length; j++)
			{
				this.yotogi_class_data_access_[j] = new MaidParam.StatusAccess.MaidClassDataAccess<int>(this.par_.status_.yotogi_class_data[j]);
			}
			this.maid_class_bonus_status_access_ = new MaidParam.StatusAccess.MaidClassBonusStatusAccess(this.par_);
			this.body_access_ = new MaidParam.StatusAccess.BodyAccess(this.par_);
			this.sexual_access_ = new MaidParam.StatusAccess.SexualAccess(this.par_);
		}

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x0600178E RID: 6030 RVA: 0x000AD26C File Offset: 0x000AB46C
		public string guid
		{
			get
			{
				return this.par_.status_.guid_id;
			}
		}

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x0600178F RID: 6031 RVA: 0x000AD280 File Offset: 0x000AB480
		public string create_time
		{
			get
			{
				return this.par_.status_.create_time;
			}
		}

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06001790 RID: 6032 RVA: 0x000AD294 File Offset: 0x000AB494
		public ulong create_time_num
		{
			get
			{
				return this.par_.status_.create_time_num;
			}
		}

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06001791 RID: 6033 RVA: 0x000AD2A8 File Offset: 0x000AB4A8
		public int employment_day
		{
			get
			{
				return this.par_.status_.employment_day;
			}
		}

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06001792 RID: 6034 RVA: 0x000AD2BC File Offset: 0x000AB4BC
		public int employment_elapsed_day
		{
			get
			{
				return GameMain.Instance.CharacterMgr.GetPlayerParam().status.days - this.par_.status_.employment_day;
			}
		}

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x06001793 RID: 6035 RVA: 0x000AD2F4 File Offset: 0x000AB4F4
		public int remaining_employment
		{
			get
			{
				if (!this.is_rental_maid)
				{
					return 0;
				}
				return RentalMaidManager.GetParameter(this.rental_maid_type).max_rental_day - this.employment_elapsed_day;
			}
		}

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x06001794 RID: 6036 RVA: 0x000AD328 File Offset: 0x000AB528
		public int maid_point
		{
			get
			{
				return this.par_.status_.maid_point;
			}
		}

		// Token: 0x1700030D RID: 781
		// (get) Token: 0x06001795 RID: 6037 RVA: 0x000AD33C File Offset: 0x000AB53C
		public string last_name
		{
			get
			{
				return this.par_.status_.last_name;
			}
		}

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x06001796 RID: 6038 RVA: 0x000AD350 File Offset: 0x000AB550
		public string first_name
		{
			get
			{
				return this.par_.status_.first_name;
			}
		}

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06001797 RID: 6039 RVA: 0x000AD364 File Offset: 0x000AB564
		public string profile
		{
			get
			{
				return this.par_.status_.profile;
			}
		}

		// Token: 0x17000310 RID: 784
		// (get) Token: 0x06001798 RID: 6040 RVA: 0x000AD378 File Offset: 0x000AB578
		public string free_comment
		{
			get
			{
				return this.par_.status_.free_comment;
			}
		}

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06001799 RID: 6041 RVA: 0x000AD38C File Offset: 0x000AB58C
		public Seikeiken init_seikeiken
		{
			get
			{
				return this.par_.status_.init_seikeiken;
			}
		}

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x0600179A RID: 6042 RVA: 0x000AD3A0 File Offset: 0x000AB5A0
		public Seikeiken seikeiken
		{
			get
			{
				return this.par_.status_.seikeiken;
			}
		}

		// Token: 0x17000313 RID: 787
		// (get) Token: 0x0600179B RID: 6043 RVA: 0x000AD3B4 File Offset: 0x000AB5B4
		public Personal personal
		{
			get
			{
				if (this.par_.status_.personal == Personal.Yandere && !GameUty.CheckPackFlag(PluginData.Type.PERSONAL001))
				{
					this.par_.status_.personal = Personal.Pure;
				}
				return this.par_.status_.personal;
			}
		}

		// Token: 0x17000314 RID: 788
		// (get) Token: 0x0600179C RID: 6044 RVA: 0x000AD404 File Offset: 0x000AB604
		public ContractType contract_type
		{
			get
			{
				return this.par_.status_.contract_type;
			}
		}

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x0600179D RID: 6045 RVA: 0x000AD418 File Offset: 0x000AB618
		public MaidClassType current_maid_class
		{
			get
			{
				return this.par_.status_.current_maid_class;
			}
		}

		// Token: 0x17000316 RID: 790
		// (get) Token: 0x0600179E RID: 6046 RVA: 0x000AD42C File Offset: 0x000AB62C
		public int current_yotogi_class
		{
			get
			{
				return this.par_.status_.current_yotogi_class;
			}
		}

		// Token: 0x17000317 RID: 791
		// (get) Token: 0x0600179F RID: 6047 RVA: 0x000AD440 File Offset: 0x000AB640
		public MaidParam.StatusAccess.BodyAccess body
		{
			get
			{
				return this.body_access_;
			}
		}

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x060017A0 RID: 6048 RVA: 0x000AD448 File Offset: 0x000AB648
		public Condition condition
		{
			get
			{
				return this.par_.status_.condition;
			}
		}

		// Token: 0x17000319 RID: 793
		// (get) Token: 0x060017A1 RID: 6049 RVA: 0x000AD45C File Offset: 0x000AB65C
		public ConditionSpecial condition_special
		{
			get
			{
				return this.par_.status_.condition_special;
			}
		}

		// Token: 0x1700031A RID: 794
		// (get) Token: 0x060017A2 RID: 6050 RVA: 0x000AD470 File Offset: 0x000AB670
		public bool is_marriage
		{
			get
			{
				return this.GetGenericFlag("_maid_param_marriage_flag") != 0;
			}
		}

		// Token: 0x1700031B RID: 795
		// (get) Token: 0x060017A3 RID: 6051 RVA: 0x000AD484 File Offset: 0x000AB684
		public bool is_newwife
		{
			get
			{
				return this.GetGenericFlag("_maid_param_newwife_flag") != 0;
			}
		}

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x060017A4 RID: 6052 RVA: 0x000AD498 File Offset: 0x000AB698
		public int yotogi_play_count
		{
			get
			{
				return this.par_.status_.yotogi_play_count;
			}
		}

		// Token: 0x1700031D RID: 797
		// (get) Token: 0x060017A5 RID: 6053 RVA: 0x000AD4AC File Offset: 0x000AB6AC
		public int others_play_count
		{
			get
			{
				return this.par_.status_.others_play_count;
			}
		}

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x060017A6 RID: 6054 RVA: 0x000AD4C0 File Offset: 0x000AB6C0
		public int likability
		{
			get
			{
				return this.par_.status_.likability;
			}
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x060017A7 RID: 6055 RVA: 0x000AD4D4 File Offset: 0x000AB6D4
		public int cur_excite
		{
			get
			{
				return Math.Min(999, this.par_.status_.cur_excite);
			}
		}

		// Token: 0x17000320 RID: 800
		// (get) Token: 0x060017A8 RID: 6056 RVA: 0x000AD4F0 File Offset: 0x000AB6F0
		public int hp
		{
			get
			{
				return Math.Min(999, this.par_.status_.hp + this.maid_class_bonus_status.hp);
			}
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x060017A9 RID: 6057 RVA: 0x000AD524 File Offset: 0x000AB724
		public int cur_hp
		{
			get
			{
				return Math.Min(this.hp, this.par_.status_.cur_hp);
			}
		}

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x060017AA RID: 6058 RVA: 0x000AD544 File Offset: 0x000AB744
		public int mind
		{
			get
			{
				return Math.Min(999, this.par_.status_.mind + this.maid_class_bonus_status.mind);
			}
		}

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x060017AB RID: 6059 RVA: 0x000AD578 File Offset: 0x000AB778
		public int cur_mind
		{
			get
			{
				return Math.Min(this.mind, this.par_.status_.cur_mind);
			}
		}

		// Token: 0x17000324 RID: 804
		// (get) Token: 0x060017AC RID: 6060 RVA: 0x000AD598 File Offset: 0x000AB798
		public int reason
		{
			get
			{
				return Math.Min(999, this.par_.status_.reason);
			}
		}

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x060017AD RID: 6061 RVA: 0x000AD5B4 File Offset: 0x000AB7B4
		public int cur_reason
		{
			get
			{
				return Math.Min(this.reason, this.par_.status_.cur_reason);
			}
		}

		// Token: 0x17000326 RID: 806
		// (get) Token: 0x060017AE RID: 6062 RVA: 0x000AD5D4 File Offset: 0x000AB7D4
		public int study_rate
		{
			get
			{
				return Math.Min(1000, this.par_.status_.study_rate);
			}
		}

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x060017AF RID: 6063 RVA: 0x000AD5F0 File Offset: 0x000AB7F0
		public int reception
		{
			get
			{
				return Math.Min(9999, this.par_.status_.reception + this.maid_class_bonus_status.reception);
			}
		}

		// Token: 0x17000328 RID: 808
		// (get) Token: 0x060017B0 RID: 6064 RVA: 0x000AD624 File Offset: 0x000AB824
		public int care
		{
			get
			{
				return Math.Min(9999, this.par_.status_.care + this.maid_class_bonus_status.care);
			}
		}

		// Token: 0x17000329 RID: 809
		// (get) Token: 0x060017B1 RID: 6065 RVA: 0x000AD658 File Offset: 0x000AB858
		public int lovely
		{
			get
			{
				return Math.Min(9999, this.par_.status_.lovely + this.maid_class_bonus_status.lovely);
			}
		}

		// Token: 0x1700032A RID: 810
		// (get) Token: 0x060017B2 RID: 6066 RVA: 0x000AD68C File Offset: 0x000AB88C
		public int inyoku
		{
			get
			{
				return Math.Min(9999, this.par_.status_.inyoku + this.maid_class_bonus_status.inyoku);
			}
		}

		// Token: 0x1700032B RID: 811
		// (get) Token: 0x060017B3 RID: 6067 RVA: 0x000AD6C0 File Offset: 0x000AB8C0
		public int elegance
		{
			get
			{
				return Math.Min(9999, this.par_.status_.elegance + this.maid_class_bonus_status.elegance);
			}
		}

		// Token: 0x1700032C RID: 812
		// (get) Token: 0x060017B4 RID: 6068 RVA: 0x000AD6F4 File Offset: 0x000AB8F4
		public int m_value
		{
			get
			{
				return Math.Min(9999, this.par_.status_.m_value + this.maid_class_bonus_status.m_value);
			}
		}

		// Token: 0x1700032D RID: 813
		// (get) Token: 0x060017B5 RID: 6069 RVA: 0x000AD728 File Offset: 0x000AB928
		public int charm
		{
			get
			{
				return Math.Min(9999, this.par_.status_.charm + this.maid_class_bonus_status.charme);
			}
		}

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x060017B6 RID: 6070 RVA: 0x000AD75C File Offset: 0x000AB95C
		public int hentai
		{
			get
			{
				return Math.Min(9999, this.par_.status_.hentai + this.maid_class_bonus_status.hentai);
			}
		}

		// Token: 0x1700032F RID: 815
		// (get) Token: 0x060017B7 RID: 6071 RVA: 0x000AD790 File Offset: 0x000AB990
		public int housi
		{
			get
			{
				return Math.Min(9999, this.par_.status_.housi + this.maid_class_bonus_status.housi);
			}
		}

		// Token: 0x17000330 RID: 816
		// (get) Token: 0x060017B8 RID: 6072 RVA: 0x000AD7C4 File Offset: 0x000AB9C4
		public int teach_rate
		{
			get
			{
				return Math.Min(1000, this.par_.status_.teach_rate + this.maid_class_bonus_status.teach_rate);
			}
		}

		// Token: 0x17000331 RID: 817
		// (get) Token: 0x060017B9 RID: 6073 RVA: 0x000AD7F8 File Offset: 0x000AB9F8
		public MaidParam.StatusAccess.SexualAccess sexual
		{
			get
			{
				return this.sexual_access_;
			}
		}

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x060017BA RID: 6074 RVA: 0x000AD800 File Offset: 0x000ABA00
		public int play_number
		{
			get
			{
				return this.par_.status_.play_number;
			}
		}

		// Token: 0x17000333 RID: 819
		// (get) Token: 0x060017BB RID: 6075 RVA: 0x000AD814 File Offset: 0x000ABA14
		public int frustration
		{
			get
			{
				return Math.Min(100, this.par_.status_.frustration);
			}
		}

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x060017BC RID: 6076 RVA: 0x000AD830 File Offset: 0x000ABA30
		public int popular_rank
		{
			get
			{
				return this.par_.status_.popular_rank;
			}
		}

		// Token: 0x17000335 RID: 821
		// (get) Token: 0x060017BD RID: 6077 RVA: 0x000AD844 File Offset: 0x000ABA44
		public int noon_work_id
		{
			get
			{
				return this.par_.status_.noon_work_id;
			}
		}

		// Token: 0x17000336 RID: 822
		// (get) Token: 0x060017BE RID: 6078 RVA: 0x000AD858 File Offset: 0x000ABA58
		public int night_work_id
		{
			get
			{
				return this.par_.status_.night_work_id;
			}
		}

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x060017BF RID: 6079 RVA: 0x000AD86C File Offset: 0x000ABA6C
		public long evaluation
		{
			get
			{
				return this.par_.status_.evaluation;
			}
		}

		// Token: 0x17000338 RID: 824
		// (get) Token: 0x060017C0 RID: 6080 RVA: 0x000AD880 File Offset: 0x000ABA80
		public long total_evaluation
		{
			get
			{
				return this.par_.status_.total_evaluation;
			}
		}

		// Token: 0x17000339 RID: 825
		// (get) Token: 0x060017C1 RID: 6081 RVA: 0x000AD894 File Offset: 0x000ABA94
		public long sales
		{
			get
			{
				return this.par_.status_.sales;
			}
		}

		// Token: 0x1700033A RID: 826
		// (get) Token: 0x060017C2 RID: 6082 RVA: 0x000AD8A8 File Offset: 0x000ABAA8
		public long total_sales
		{
			get
			{
				return this.par_.status_.total_sales;
			}
		}

		// Token: 0x1700033B RID: 827
		// (get) Token: 0x060017C3 RID: 6083 RVA: 0x000AD8BC File Offset: 0x000ABABC
		public bool is_first_name_call
		{
			get
			{
				return this.par_.status_.is_first_name_call;
			}
		}

		// Token: 0x1700033C RID: 828
		// (get) Token: 0x060017C4 RID: 6084 RVA: 0x000AD8D0 File Offset: 0x000ABAD0
		public bool is_rental_maid
		{
			get
			{
				return this.par_.status_.is_rental_maid;
			}
		}

		// Token: 0x1700033D RID: 829
		// (get) Token: 0x060017C5 RID: 6085 RVA: 0x000AD8E4 File Offset: 0x000ABAE4
		public RentalMaidManager.Chara rental_maid_type
		{
			get
			{
				if (!this.is_rental_maid)
				{
					return RentalMaidManager.Chara.Max;
				}
				RentalMaidManager.Chara result = RentalMaidManager.Chara.Max;
				try
				{
					result = (RentalMaidManager.Chara)((int)Enum.Parse(typeof(RentalMaidManager.Chara), this.first_name));
				}
				catch (ArgumentException)
				{
					Debug.Log("RentalMaidManager.Chara - enum parse error\n[" + this.first_name + "]");
				}
				return result;
			}
		}

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x060017C6 RID: 6086 RVA: 0x000AD960 File Offset: 0x000ABB60
		public int banishment_price
		{
			get
			{
				return 10000;
			}
		}

		// Token: 0x1700033F RID: 831
		// (get) Token: 0x060017C7 RID: 6087 RVA: 0x000AD968 File Offset: 0x000ABB68
		public Dictionary<string, int> generic_flag
		{
			get
			{
				return this.par_.status_.generic_flag;
			}
		}

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x060017C8 RID: 6088 RVA: 0x000AD97C File Offset: 0x000ABB7C
		public Dictionary<string, string> parts_dic
		{
			get
			{
				return this.par_.status_.parts_dic;
			}
		}

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x060017C9 RID: 6089 RVA: 0x000AD990 File Offset: 0x000ABB90
		public bool employment
		{
			get
			{
				return this.par_.status_.employment;
			}
		}

		// Token: 0x17000342 RID: 834
		// (get) Token: 0x060017CA RID: 6090 RVA: 0x000AD9A4 File Offset: 0x000ABBA4
		public bool leader
		{
			get
			{
				return this.par_.status_.leader;
			}
		}

		// Token: 0x17000343 RID: 835
		// (get) Token: 0x060017CB RID: 6091 RVA: 0x000AD9B8 File Offset: 0x000ABBB8
		public EyePartsTab eye_parts_tab
		{
			get
			{
				return this.par_.status_.eye_parts_tab;
			}
		}

		// Token: 0x17000344 RID: 836
		// (get) Token: 0x060017CC RID: 6092 RVA: 0x000AD9CC File Offset: 0x000ABBCC
		public int inran_gap
		{
			get
			{
				return this.inyoku + this.lovely;
			}
		}

		// Token: 0x17000345 RID: 837
		// (get) Token: 0x060017CD RID: 6093 RVA: 0x000AD9DC File Offset: 0x000ABBDC
		public int m_value_gap
		{
			get
			{
				return this.m_value + this.elegance;
			}
		}

		// Token: 0x17000346 RID: 838
		// (get) Token: 0x060017CE RID: 6094 RVA: 0x000AD9EC File Offset: 0x000ABBEC
		public int hentai_gap
		{
			get
			{
				return this.hentai + this.charm;
			}
		}

		// Token: 0x060017CF RID: 6095 RVA: 0x000AD9FC File Offset: 0x000ABBFC
		public HashSet<Feature> GetFeatureList()
		{
			return new HashSet<Feature>(this.par_.status_.feature);
		}

		// Token: 0x060017D0 RID: 6096 RVA: 0x000ADA14 File Offset: 0x000ABC14
		public bool IsFeature(Feature check_feature)
		{
			return this.par_.status_.feature.Contains(check_feature);
		}

		// Token: 0x060017D1 RID: 6097 RVA: 0x000ADA2C File Offset: 0x000ABC2C
		public HashSet<Propensity> GetPropensityList()
		{
			return new HashSet<Propensity>(this.par_.status_.propensity);
		}

		// Token: 0x060017D2 RID: 6098 RVA: 0x000ADA44 File Offset: 0x000ABC44
		public bool IsPropensity(Propensity check_propensity)
		{
			return this.par_.status_.propensity.Contains(check_propensity);
		}

		// Token: 0x060017D3 RID: 6099 RVA: 0x000ADA5C File Offset: 0x000ABC5C
		public bool IsGetSkill(int skill_id)
		{
			return this.par_.status_.skill_data.ContainsKey(skill_id);
		}

		// Token: 0x060017D4 RID: 6100 RVA: 0x000ADA74 File Offset: 0x000ABC74
		public bool IsGetWork(int work_id)
		{
			return this.par_.status_.work_data.ContainsKey(work_id);
		}

		// Token: 0x060017D5 RID: 6101 RVA: 0x000ADA8C File Offset: 0x000ABC8C
		public int GetGenericFlag(string flag_name)
		{
			return (!this.par_.status_.generic_flag.ContainsKey(flag_name)) ? 0 : this.par_.status_.generic_flag[flag_name];
		}

		// Token: 0x060017D6 RID: 6102 RVA: 0x000ADAC8 File Offset: 0x000ABCC8
		public int[] GetSkillIdArray()
		{
			int[] array = new int[this.par_.status_.skill_data.Count];
			int num = 0;
			foreach (KeyValuePair<int, param.Status.SkillData> keyValuePair in this.par_.status_.skill_data)
			{
				array[num] = keyValuePair.Key;
				num++;
			}
			return array;
		}

		// Token: 0x060017D7 RID: 6103 RVA: 0x000ADB60 File Offset: 0x000ABD60
		public MaidParam.StatusAccess.SkillDataAccess GetSkillData(int skill_id)
		{
			NDebug.Assert(this.IsGetSkill(skill_id), "未取得のスキルID[" + skill_id.ToString() + "]のデータを参照しようとしました");
			return new MaidParam.StatusAccess.SkillDataAccess(this.par_.status_.skill_data[skill_id]);
		}

		// Token: 0x060017D8 RID: 6104 RVA: 0x000ADBAC File Offset: 0x000ABDAC
		public int[] GetWorkIdArray()
		{
			int[] array = new int[this.par_.status_.work_data.Count];
			int num = 0;
			foreach (KeyValuePair<int, param.Status.WorkData> keyValuePair in this.par_.status_.work_data)
			{
				array[num] = keyValuePair.Key;
				num++;
			}
			return array;
		}

		// Token: 0x060017D9 RID: 6105 RVA: 0x000ADC44 File Offset: 0x000ABE44
		public MaidParam.StatusAccess.WorkDataAccess GetWorkData(int work_id)
		{
            Debug.Assert(this.IsGetWork(work_id), "未取得の仕事ID[" + work_id.ToString() + "]のデータを参照しようとしました");
			return new MaidParam.StatusAccess.WorkDataAccess(this.par_.status_.work_data[work_id]);
		}

		// Token: 0x060017DA RID: 6106 RVA: 0x000ADC90 File Offset: 0x000ABE90
		public MaidParam.StatusAccess.MaidClassDataAccess<MaidClassType>[] GetMaidClassDataArray()
		{
			return this.maid_class_data_access_;
		}

		// Token: 0x060017DB RID: 6107 RVA: 0x000ADC98 File Offset: 0x000ABE98
		public MaidParam.StatusAccess.MaidClassDataAccess<MaidClassType> GetMaidClassData(MaidClassType maid_class_type)
		{
			return this.maid_class_data_access_[(int)maid_class_type];
		}

		// Token: 0x060017DC RID: 6108 RVA: 0x000ADCA4 File Offset: 0x000ABEA4
		public MaidParam.StatusAccess.MaidClassDataAccess<int>[] GetYotogiClassDataArray()
		{
			return this.yotogi_class_data_access_;
		}

		// Token: 0x060017DD RID: 6109 RVA: 0x000ADCAC File Offset: 0x000ABEAC
		public MaidParam.StatusAccess.MaidClassDataAccess<int> GetYotogiClassData(int yotogi_class_type)
		{
			return this.yotogi_class_data_access_[yotogi_class_type];
		}

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x060017DE RID: 6110 RVA: 0x000ADCB8 File Offset: 0x000ABEB8
		public MaidParam.StatusAccess.MaidClassBonusStatusAccess maid_class_bonus_status
		{
			get
			{
				return this.maid_class_bonus_status_access_;
			}
		}

		// Token: 0x17000348 RID: 840
		// (get) Token: 0x060017DF RID: 6111 RVA: 0x000ADCC0 File Offset: 0x000ABEC0
		public Correction.Data correction_data
		{
			get
			{
				return Correction.GetData(this.par_);
			}
		}

		// Token: 0x040014AD RID: 5293
		private readonly MaidParam.StatusAccess.MaidClassDataAccess<MaidClassType>[] maid_class_data_access_;

		// Token: 0x040014AE RID: 5294
		private readonly MaidParam.StatusAccess.MaidClassDataAccess<int>[] yotogi_class_data_access_;

		// Token: 0x040014AF RID: 5295
		private readonly MaidParam.StatusAccess.MaidClassBonusStatusAccess maid_class_bonus_status_access_;

		// Token: 0x040014B0 RID: 5296
		private readonly MaidParam.StatusAccess.BodyAccess body_access_;

		// Token: 0x040014B1 RID: 5297
		private readonly MaidParam.StatusAccess.SexualAccess sexual_access_;

		// Token: 0x040014B2 RID: 5298
		private readonly MaidParam par_;

		// Token: 0x020002EA RID: 746
		public class MaidClassDataAccess<T>
		{
			// Token: 0x060017E0 RID: 6112 RVA: 0x000ADCD0 File Offset: 0x000ABED0
			public MaidClassDataAccess(param.Status.MaidClassData<T> class_data)
			{
				this.class_data_ = class_data;
			}

			// Token: 0x17000349 RID: 841
			// (get) Token: 0x060017E1 RID: 6113 RVA: 0x000ADCE0 File Offset: 0x000ABEE0
			public T type
			{
				get
				{
					return this.class_data_.type;
				}
			}

			// Token: 0x1700034A RID: 842
			// (get) Token: 0x060017E2 RID: 6114 RVA: 0x000ADCF0 File Offset: 0x000ABEF0
			public int level
			{
				get
				{
					return this.class_data_.level;
				}
			}

			// Token: 0x1700034B RID: 843
			// (get) Token: 0x060017E3 RID: 6115 RVA: 0x000ADD00 File Offset: 0x000ABF00
			public int cur_exp
			{
				get
				{
					return this.class_data_.cur_exp;
				}
			}

			// Token: 0x1700034C RID: 844
			// (get) Token: 0x060017E4 RID: 6116 RVA: 0x000ADD10 File Offset: 0x000ABF10
			public int next_exp
			{
				get
				{
					return this.class_data_.next_exp;
				}
			}

			// Token: 0x1700034D RID: 845
			// (get) Token: 0x060017E5 RID: 6117 RVA: 0x000ADD20 File Offset: 0x000ABF20
			public int total_exp
			{
				get
				{
					return this.class_data_.exp_system.GetTotalExp();
				}
			}

			// Token: 0x1700034E RID: 846
			// (get) Token: 0x060017E6 RID: 6118 RVA: 0x000ADD34 File Offset: 0x000ABF34
			public bool is_have
			{
				get
				{
					return this.class_data_.is_have;
				}
			}

			// Token: 0x1700034F RID: 847
			// (get) Token: 0x060017E7 RID: 6119 RVA: 0x000ADD44 File Offset: 0x000ABF44
			public int max_level
			{
				get
				{
					return this.class_data_.exp_system.GetMaxLevel();
				}
			}

			// Token: 0x060017E8 RID: 6120 RVA: 0x000ADD58 File Offset: 0x000ABF58
			public param.Status.MaidClassData<T> GetData()
			{
				return this.class_data_;
			}

			// Token: 0x040014B3 RID: 5299
			private readonly param.Status.MaidClassData<T> class_data_;
		}

		// Token: 0x020002EB RID: 747
		public class SkillDataAccess
		{
			// Token: 0x060017E9 RID: 6121 RVA: 0x000ADD60 File Offset: 0x000ABF60
			public SkillDataAccess(param.Status.SkillData skill_data)
			{
				this.skill_data_ = skill_data;
			}

			// Token: 0x17000350 RID: 848
			// (get) Token: 0x060017EA RID: 6122 RVA: 0x000ADD70 File Offset: 0x000ABF70
			public int id
			{
				get
				{
					return this.skill_data_.id;
				}
			}

			// Token: 0x17000351 RID: 849
			// (get) Token: 0x060017EB RID: 6123 RVA: 0x000ADD80 File Offset: 0x000ABF80
			public uint play_count
			{
				get
				{
					return this.skill_data_.play_count;
				}
			}

			// Token: 0x17000352 RID: 850
			// (get) Token: 0x060017EC RID: 6124 RVA: 0x000ADD90 File Offset: 0x000ABF90
			public int level
			{
				get
				{
					return this.skill_data_.level;
				}
			}

			// Token: 0x17000353 RID: 851
			// (get) Token: 0x060017ED RID: 6125 RVA: 0x000ADDA0 File Offset: 0x000ABFA0
			public int cur_exp
			{
				get
				{
					return this.skill_data_.cur_exp;
				}
			}

			// Token: 0x17000354 RID: 852
			// (get) Token: 0x060017EE RID: 6126 RVA: 0x000ADDB0 File Offset: 0x000ABFB0
			public int next_exp
			{
				get
				{
					return this.skill_data_.next_exp;
				}
			}

			// Token: 0x17000355 RID: 853
			// (get) Token: 0x060017EF RID: 6127 RVA: 0x000ADDC0 File Offset: 0x000ABFC0
			public int max_level
			{
				get
				{
					return this.skill_data_.exp_system.GetMaxLevel();
				}
			}

			// Token: 0x060017F0 RID: 6128 RVA: 0x000ADDD4 File Offset: 0x000ABFD4
			public param.Status.SkillData GetData()
			{
				return this.skill_data_;
			}

			// Token: 0x040014B4 RID: 5300
			private readonly param.Status.SkillData skill_data_;
		}

		// Token: 0x020002EC RID: 748
		public class WorkDataAccess
		{
			// Token: 0x060017F1 RID: 6129 RVA: 0x000ADDDC File Offset: 0x000ABFDC
			public WorkDataAccess(param.Status.WorkData work_data)
			{
				this.work_data_ = work_data;
			}

			// Token: 0x17000356 RID: 854
			// (get) Token: 0x060017F2 RID: 6130 RVA: 0x000ADDEC File Offset: 0x000ABFEC
			public int id
			{
				get
				{
					return this.work_data_.id;
				}
			}

			// Token: 0x17000357 RID: 855
			// (get) Token: 0x060017F3 RID: 6131 RVA: 0x000ADDFC File Offset: 0x000ABFFC
			public uint play_count
			{
				get
				{
					return this.work_data_.play_count;
				}
			}

			// Token: 0x17000358 RID: 856
			// (get) Token: 0x060017F4 RID: 6132 RVA: 0x000ADE0C File Offset: 0x000AC00C
			public int level
			{
				get
				{
					return this.work_data_.level;
				}
			}

			// Token: 0x060017F5 RID: 6133 RVA: 0x000ADE1C File Offset: 0x000AC01C
			public param.Status.WorkData GetData()
			{
				return this.work_data_;
			}

			// Token: 0x040014B5 RID: 5301
			private readonly param.Status.WorkData work_data_;
		}

		// Token: 0x020002ED RID: 749
		public class MaidClassBonusStatusAccess
		{
			// Token: 0x060017F6 RID: 6134 RVA: 0x000ADE24 File Offset: 0x000AC024
			public MaidClassBonusStatusAccess(MaidParam par)
			{
				this.par_ = par;
			}

			// Token: 0x17000359 RID: 857
			// (get) Token: 0x060017F7 RID: 6135 RVA: 0x000ADE34 File Offset: 0x000AC034
			public int hp
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.hp;
				}
			}

			// Token: 0x1700035A RID: 858
			// (get) Token: 0x060017F8 RID: 6136 RVA: 0x000ADE4C File Offset: 0x000AC04C
			public int mind
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.mind;
				}
			}

			// Token: 0x1700035B RID: 859
			// (get) Token: 0x060017F9 RID: 6137 RVA: 0x000ADE64 File Offset: 0x000AC064
			public int reception
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.reception;
				}
			}

			// Token: 0x1700035C RID: 860
			// (get) Token: 0x060017FA RID: 6138 RVA: 0x000ADE7C File Offset: 0x000AC07C
			public int care
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.care;
				}
			}

			// Token: 0x1700035D RID: 861
			// (get) Token: 0x060017FB RID: 6139 RVA: 0x000ADE94 File Offset: 0x000AC094
			public int lovely
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.lovely;
				}
			}

			// Token: 0x1700035E RID: 862
			// (get) Token: 0x060017FC RID: 6140 RVA: 0x000ADEAC File Offset: 0x000AC0AC
			public int inyoku
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.inyoku;
				}
			}

			// Token: 0x1700035F RID: 863
			// (get) Token: 0x060017FD RID: 6141 RVA: 0x000ADEC4 File Offset: 0x000AC0C4
			public int elegance
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.elegance;
				}
			}

			// Token: 0x17000360 RID: 864
			// (get) Token: 0x060017FE RID: 6142 RVA: 0x000ADEDC File Offset: 0x000AC0DC
			public int m_value
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.m_value;
				}
			}

			// Token: 0x17000361 RID: 865
			// (get) Token: 0x060017FF RID: 6143 RVA: 0x000ADEF4 File Offset: 0x000AC0F4
			public int charme
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.charm;
				}
			}

			// Token: 0x17000362 RID: 866
			// (get) Token: 0x06001800 RID: 6144 RVA: 0x000ADF0C File Offset: 0x000AC10C
			public int hentai
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.hentai;
				}
			}

			// Token: 0x17000363 RID: 867
			// (get) Token: 0x06001801 RID: 6145 RVA: 0x000ADF24 File Offset: 0x000AC124
			public int housi
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.housi;
				}
			}

			// Token: 0x17000364 RID: 868
			// (get) Token: 0x06001802 RID: 6146 RVA: 0x000ADF3C File Offset: 0x000AC13C
			public int teach_rate
			{
				get
				{
					return this.par_.status_.maid_class_bonus_status.teach_rate;
				}
			}

			// Token: 0x040014B6 RID: 5302
			private readonly MaidParam par_;
		}

		// Token: 0x020002EE RID: 750
		public class BodyAccess
		{
			// Token: 0x06001803 RID: 6147 RVA: 0x000ADF54 File Offset: 0x000AC154
			public BodyAccess(MaidParam par)
			{
				this.par_ = par;
			}

			// Token: 0x17000365 RID: 869
			// (get) Token: 0x06001804 RID: 6148 RVA: 0x000ADF64 File Offset: 0x000AC164
			public int height
			{
				get
				{
					return this.par_.status_.body.height;
				}
			}

			// Token: 0x17000366 RID: 870
			// (get) Token: 0x06001805 RID: 6149 RVA: 0x000ADF7C File Offset: 0x000AC17C
			public int weight
			{
				get
				{
					return this.par_.status_.body.weight;
				}
			}

			// Token: 0x17000367 RID: 871
			// (get) Token: 0x06001806 RID: 6150 RVA: 0x000ADF94 File Offset: 0x000AC194
			public int bust
			{
				get
				{
					return this.par_.status_.body.bust;
				}
			}

			// Token: 0x17000368 RID: 872
			// (get) Token: 0x06001807 RID: 6151 RVA: 0x000ADFAC File Offset: 0x000AC1AC
			public int waist
			{
				get
				{
					return this.par_.status_.body.waist;
				}
			}

			// Token: 0x17000369 RID: 873
			// (get) Token: 0x06001808 RID: 6152 RVA: 0x000ADFC4 File Offset: 0x000AC1C4
			public int hip
			{
				get
				{
					return this.par_.status_.body.hip;
				}
			}

			// Token: 0x1700036A RID: 874
			// (get) Token: 0x06001809 RID: 6153 RVA: 0x000ADFDC File Offset: 0x000AC1DC
			public string cup
			{
				get
				{
					return this.par_.status_.body.cup;
				}
			}

			// Token: 0x0600180A RID: 6154 RVA: 0x000ADFF4 File Offset: 0x000AC1F4
			public param.Status.Body GetStruct()
			{
				return this.par_.status_.body;
			}

			// Token: 0x040014B7 RID: 5303
			private readonly MaidParam par_;
		}

		// Token: 0x020002EF RID: 751
		public class SexualAccess
		{
			// Token: 0x0600180B RID: 6155 RVA: 0x000AE008 File Offset: 0x000AC208
			public SexualAccess(MaidParam par)
			{
				this.par_ = par;
			}

			// Token: 0x1700036B RID: 875
			// (get) Token: 0x0600180C RID: 6156 RVA: 0x000AE018 File Offset: 0x000AC218
			public int mouth
			{
				get
				{
					return this.par_.status_.sexual.mouth;
				}
			}

			// Token: 0x1700036C RID: 876
			// (get) Token: 0x0600180D RID: 6157 RVA: 0x000AE030 File Offset: 0x000AC230
			public int throat
			{
				get
				{
					return this.par_.status_.sexual.throat;
				}
			}

			// Token: 0x1700036D RID: 877
			// (get) Token: 0x0600180E RID: 6158 RVA: 0x000AE048 File Offset: 0x000AC248
			public int nipple
			{
				get
				{
					return this.par_.status_.sexual.nipple;
				}
			}

			// Token: 0x1700036E RID: 878
			// (get) Token: 0x0600180F RID: 6159 RVA: 0x000AE060 File Offset: 0x000AC260
			public int front
			{
				get
				{
					return this.par_.status_.sexual.front;
				}
			}

			// Token: 0x1700036F RID: 879
			// (get) Token: 0x06001810 RID: 6160 RVA: 0x000AE078 File Offset: 0x000AC278
			public int back
			{
				get
				{
					return this.par_.status_.sexual.back;
				}
			}

			// Token: 0x17000370 RID: 880
			// (get) Token: 0x06001811 RID: 6161 RVA: 0x000AE090 File Offset: 0x000AC290
			public int curi
			{
				get
				{
					return this.par_.status_.sexual.curi;
				}
			}

			// Token: 0x06001812 RID: 6162 RVA: 0x000AE0A8 File Offset: 0x000AC2A8
			public param.Status.Sexual GetStruct()
			{
				return this.par_.status_.sexual;
			}

			// Token: 0x040014B8 RID: 5304
			private readonly MaidParam par_;
		}
	}

	// Token: 0x020002F0 RID: 752
	private class DataBlock
	{
		// Token: 0x06001814 RID: 6164 RVA: 0x000AE0C4 File Offset: 0x000AC2C4
		public int GetOriginalX(int x)
		{
			return this.rect.left + x;
		}

		// Token: 0x06001815 RID: 6165 RVA: 0x000AE0D4 File Offset: 0x000AC2D4
		public int GetOriginalY(int y)
		{
			return this.rect.top + y;
		}

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x06001816 RID: 6166 RVA: 0x000AE0E4 File Offset: 0x000AC2E4
		public int max_x
		{
			get
			{
				return this.rect.right - this.rect.left + 1;
			}
		}

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x06001817 RID: 6167 RVA: 0x000AE100 File Offset: 0x000AC300
		public int max_y
		{
			get
			{
				return this.rect.bottom - this.rect.top + 1;
			}
		}

		// Token: 0x040014B9 RID: 5305
		public PlaneRect<int> rect;
	}

	// Token: 0x020002F1 RID: 753
	private class CsvDataBlock : MaidParam.DataBlock
	{
		// Token: 0x06001818 RID: 6168 RVA: 0x000AE11C File Offset: 0x000AC31C
		public CsvDataBlock(CsvParser check_csv, int check_start_y)
		{
			this.csv_ = check_csv;
			this.NextBlock(check_start_y);
		}

		// Token: 0x06001819 RID: 6169 RVA: 0x000AE134 File Offset: 0x000AC334
		public bool NextBlock(int check_start_y)
		{
			this.rect.left = 0;
			this.rect.bottom = this.csv_.max_cell_y - 1;
			for (int i = check_start_y; i < this.csv_.max_cell_y; i++)
			{
				if (this.csv_.IsCellToExistData(0, i))
				{
					this.rect.top = i;
					int j;
					for (j = 0; j < this.csv_.max_cell_x; j++)
					{
						if (!this.csv_.IsCellToExistData(j, i))
						{
							break;
						}
					}
					j--;
					this.rect.right = j;
					break;
				}
			}
			if (this.rect.right <= 0)
			{
				this.rect.left = (this.rect.right = (this.rect.top = (this.rect.bottom = 0)));
				return false;
			}
			for (int k = this.rect.top; k < this.csv_.max_cell_y; k++)
			{
				bool flag = false;
				for (int l = 0; l <= this.rect.right; l++)
				{
					if (this.csv_.IsCellToExistData(l, k))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.rect.bottom = k - 1;
					break;
				}
			}
			for (int m = this.rect.left; m < this.csv_.max_cell_x; m++)
			{
				for (int n = this.rect.top; n <= this.rect.bottom; n++)
				{
					if (this.csv_.IsCellToExistData(m, n) && this.rect.right < m - 1)
					{
						this.rect.right = m - 1;
					}
				}
			}
			return true;
		}

		// Token: 0x0600181A RID: 6170 RVA: 0x000AE344 File Offset: 0x000AC544
		public void BlockAnalysis(int start_y, Func<MaidParam.CsvDataBlock, int, bool> line_func, Func<MaidParam.CsvDataBlock, int, int, bool> success_call_back)
		{
			for (int i = start_y; i < base.max_y; i++)
			{
				if (line_func(this, base.GetOriginalY(i)) && success_call_back(this, this.rect.left, i))
				{
					return;
				}
			}
		}

		// Token: 0x0600181B RID: 6171 RVA: 0x000AE394 File Offset: 0x000AC594
		public bool NextBlock()
		{
			return this.NextBlock(base.GetOriginalY(base.max_y));
		}

		// Token: 0x17000373 RID: 883
		// (get) Token: 0x0600181C RID: 6172 RVA: 0x000AE3A8 File Offset: 0x000AC5A8
		public CsvParser csv
		{
			get
			{
				return this.csv_;
			}
		}

		// Token: 0x040014BA RID: 5306
		private CsvParser csv_;
	}
}
