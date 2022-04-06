using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using WIA;
using ZXing;
using Point = System.Drawing.Point;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace 屏幕识文
{
    /// <summary>
    /// FullScreenWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FullScreenWindow : System.Windows.Window, INotifyPropertyChanged
    {
       
        private enum MouseAction
        {
            Nothing, DrawRect, MoveRect
        }

        #region Commands
        public ICommand CloseCommand { get; set; }
        public ICommand MouseDClickCommand { get; set; }
        public ICommand UnselectCommand { get; set; }

        // OCR 命令
        public ICommand OcrGeneralBasicCommand { get; set; }
        public ICommand OcrAccurateCommand { get; set; }
        public ICommand OcrGeneralEnhancedCommand { get; set; }
        public ICommand OcrBankCardCommand { get; set; }
        public ICommand OcrIdcardCommand { get; set; }
        public ICommand OcrDrivingLicenseCommand { get; set; }
        public ICommand OcrVehicleLicenseCommand { get; set; }
        public ICommand OcrPlateLicenseCommand { get; set; }
        public ICommand OcrBusinessLicenseCommand { get; set; }
        public ICommand OcrReceiptCommand { get; set; }
        public ICommand OcrFormCommand { get; set; }
        public ICommand ScanCodeCommand { get; set; }
        public ICommand HandWritingCommand { get; set; }
        public ICommand PassportCommand { get; set; }
        public ICommand LotteryCommand { get; set; }
        public ICommand ScanningCommand { get; set; }
        public ICommand TrainticketsCommand { get; set; }
        public ICommand NumberCommand { get; set; }
        public ICommand InsuranceCommand { get; set; }
        public ICommand TaxiReceiptCommand { get; set; }
        public ICommand VIN码Command { get; set; }
        public ICommand QuotaInvoiceCommand { get; set; }
        public ICommand VatInvoiceCommand { get; set; }
        #endregion

        // 识别所支持的语言列表
        private static string[] languages = new string[]
        {
            "CHN_ENG",
            "ENG",
            "POR",
            "FRE",
            "GER",
            "ITA",
            "SPA",
            "RUS",
            "JAP",
            "KOR"
        };

        public FullScreenWindow()
        {
            InitializeComponent();
            //全屏并且去掉边框
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;

            //关闭对象命令
            CloseCommand = new RelayCommand(() =>
            {
                Close();
            });

            //鼠标双击命令
            MouseDClickCommand = new RelayCommand(() =>
            {
                if (HitWhitch == HitWhitchTypes.rect_box)
                {
                    UnselectArea();
                }
                else
                {
                    Close();
                }
            });
            //取消选择命令
            UnselectCommand = new RelayCommand(() =>
            {
                UnselectArea();
            });


            #region Ocr 命令
            //识别文字
            OcrGeneralBasicCommand = new RelayCommand(() =>
            {
                try
                {
                    // 获取选择区域的图片数据
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        // 如果图片为空，则告知用户
                        MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 识别参数：语言
                    var options = new Dictionary<string, object>();
                    var language = languages[cmb_language.SelectedIndex];
                    options.Add("language_type", language);

                    // 识别文字，得到识别结果
                    var result = BaiduOcrClient.GeneralBasic(image, options);

                    // 判断是否正确识别，如果检测到是被错误，则弹出提示框并返回false
                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    // 获取识别结果的行数
                    var words_result_num = (int)result["words_result_num"];

                    if (words_result_num == 0)
                    {
                        // 行数为 0，没有结果
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        // 提取识别结果数组中的 words 字段，将其合并为一个字符串，作为显示的结果
                        var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()).ToArray());
                        // HideCommandBar();
                        // 显示识别结果
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    // 捕捉异常并显示
                    MessageBox.Show(e.Message, "程序错误");
                }
            });

            // 识别手写文字
            HandWritingCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    // 识别参数：语言
                    var options = new Dictionary<string, object>();
                    var language = languages[cmb_language.SelectedIndex];
                    options.Add("language_type", language);
                    //options.Add("words_type", "number");
                    // 获取识别结果的行数
                    var result = BaiduOcrClient.Handwriting(image, options);

                    // 判断是否正确识别，如果检测到是被错误，则弹出提示框并返回false
                    if (TextErrorHandler(result))
                    {
                        return;
                    }
                    var words_result_num = (int)result["words_result_num"];
                    if (words_result_num == 0)
                    {
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()).ToArray());
                        HideCommandBar();
                        ShowText(text);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("网络出现异常,请检查网络！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // 识别护照
            PassportCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 获取识别结果的行数
                    var result = BaiduOcrClient.Passport(image);

                    // 判断是否正确识别，如果检测到是被错误，则弹出提示框并返回false
                    if (TextErrorHandler(result))
                    {
                        return;
                    }
                    var words_result_num = (int)result["words_result_num"];
                    if (words_result_num == 0)
                    {
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var lines = new List<string>();
                        foreach (JProperty item in result["words_result"].Children())
                        {
                            lines.Add(item.Name + "：" + item.Value["words"]);
                        }
                        var text = string.Join("\n", lines);
                        HideCommandBar();
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            //彩票识别
            LotteryCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var result = BaiduOcrClient.Lottery(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }
                    var words_result_num = (int)result["words_result_num"];
                    if (words_result_num == 0)
                    {
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()).ToArray());
                        HideCommandBar();
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            //火车票
            TrainticketsCommand = new RelayCommand(() =>
            {
                try
                {
                    // 获取选择区域的图片数据
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        // 如果图片为空，则告知用户
                        MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 识别文字，得到识别结果
                    var result = BaiduOcrClient.TrainTicket(image);

                    // 判断是否正确识别，如果检测到是被错误，则弹出提示框并返回false
                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    // 获取识别结果的行数
                    var words_result_num = (int)result["words_result_num"];

                    if (words_result_num == 0)
                    {
                        // 行数为 0，没有结果
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        // 提取识别结果数组中的 words 字段，将其合并为一个字符串，作为显示的结果
                        // var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()).ToArray());

                        var words_result = result["words_result"];
                        var text = "";
                        text += "车票号：" + words_result.Value<string>("ticket_num") + "\n";
                        text += "始发站：" + words_result.Value<string>("starting_station") + "\n";
                        text += "车次号：" + words_result.Value<string>("train_num") + "\n";
                        text += "到达站：" + words_result.Value<string>("destination_station") + "\n";
                        text += "出发日期：" + words_result.Value<string>("date") + "\n";
                        text += "车票金额：" + words_result.Value<string>("ticket_rates") + "\n";
                        text += "席别：" + words_result.Value<string>("seat_category") + "\n";
                        text += "乘客姓名：" + words_result.Value<string>("name");
                        HideCommandBar();
                        // 显示识别结果
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            //数字识别
            NumberCommand = new RelayCommand(() =>
            {
                ShowcallWrite();
            });

            //保单识别
            InsuranceCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var result = BaiduOcrClient.Accurate(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }
                    var words_result_num = (object)result["words_result_num"];
                    if (words_result_num == null)
                    {
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()));

                       // var words_result = result["words_result"];
                        //var text = "";
                        //text += "受益人信息：" + words_result.Value<object>("BenPerLst") + "\n";
                        //text += "受益人姓名：" + words_result.Value<string>("BenCltNa") + "\n";
                        //text += "受益比例：" + words_result.Value<string>("BenPerPro") + "\n";
                        //text += "受益顺序：" + words_result.Value<string>("BenPerOrd") + "\n";
                        //text += "受益人类型：" + words_result.Value<string>("BenPerTyp") + "\n";
                        //text += "公司名称：" +   words_result.Value<string>("InsBilCom") + "\n";
                        //text += "保险单号码：" + words_result.Value<string>("InsBilNo") + "\n";
                        //text += "受益顺序：" +   words_result.Value<string>("BenPerOrd") + "\n";
                        //text += "投保人性别：" + words_result.Value<string>("InsCltGd1") + "\n";
                        //text += "投保人：" + words_result.Value<string>("InsCltNa1") + "\n";
                        //text += "投保人证件号码：" + words_result.Value<string>("InsIdcNb1") + "\n";
                        //text += "投保人证件类型：" + words_result.Value<string>("InsIdcTy1") + "\n";
                        //text += "被保人信息：" +     words_result.Value<object>("InsPerLst") + "\n";
                        //text += "被保人性别：" +     words_result.Value<string>("InsCltGd2") + "\n";
                        //text += "被保险人：" + words_result.Value<string>("InsCltNa2") + "\n";
                        //text += "被保险人出生日期：" + words_result.Value<string>("InsBthDa2") + "\n";
                        //text += "被保险人证件号码：" + words_result.Value<string>("InsIdcNb2") + "\n";
                        //text += "被保险人证件类型：" + words_result.Value<string>("InsIdcTy2") + "\n";
                        //text += "保险信息：" + words_result.Value<object>("InsPrdList") + "\n";
                        //text += "保险期限：" + words_result.Value<string>("InsCovDur") + "\n";
                        //text += "基本保险金额：" + words_result.Value<string>("InsIcvAmt") + "\n";
                        //text += "交费期间：" + words_result.Value<string>("InsPayDur") + "\n";
                        //text += "缴费频率：" + words_result.Value<string>("InsPayFeq") + "\n";
                        //text += "每期交费金额：" + words_result.Value<string>("InsPerAmt	") + "\n";
                        //text += "产品名称：" + words_result.Value<string>("InsPrdNam");
                        HideCommandBar();
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            //出租车
            TaxiReceiptCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        return;
                    }

                    var result = BaiduOcrClient.TaxiReceipt(image);
                    
                    if (TextErrorHandler(result))
                    {
                        return;   
                    }

                    var words_result_num = (int)result["words_result_num"];
                    if (words_result_num == 0)
                    {
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else
                    {
                        var words_result = result["words_result"];
                        var text = "";
                        text += "日期：" + words_result.Value<string>("Date") + "\n";
                        text += "实付金额：" + words_result.Value<string>("Fare") + "\n";
                        text += "发票代号：" + words_result.Value<string>("InvoiceCode") + "\n";
                        text += "发票号码：" + words_result.Value<string>("InvoiceNum") + "\n";
                        text += "车牌号：" + words_result.Value<string>("TaxiNum") + "\n";
                        text += "上下车时间：" + words_result.Value<string>("Time") + "\n";
                        text += "油费：" + words_result.Value<string>("FuelOilSurcharge") + "\n";
                        text += "呼叫服务附加费：" + words_result.Value<string>("CallServiceSurcharge");
                        HideCommandBar();
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            //定额发票识别
            QuotaInvoiceCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var result = BaiduOcrClient.QuotaInvoice(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }
                    var words_result_num = (int)result["words_result_num"];
                    if (words_result_num == 0)
                    {
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var words_result = result["words_result"];
                        var text = "";
                        text += "发票代码：" + words_result.Value<string>("invoice_code") + "\n";
                        text += "发票号码：" + words_result.Value<string>("invoice_number") + "\n";
                        text += "金额：" + words_result.Value<string>("invoice_rate");
                       // var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()));
                        HideCommandBar();
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            //定额增值发票识别
            VatInvoiceCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var result = BaiduOcrClient.VatInvoice(image);
                    
                    if (TextErrorHandler(result))
                    {
                        return;
                    }
                    var words_result_num = (int)result["words_result_num"];
                    if (words_result_num == 0)
                    {
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var words_result = result["words_result"];
                        var text = "";
                        text += "发票种类名称：" + words_result.Value<string>("InvoiceType") + "\n";
                        text += "发票代码：" + words_result.Value<string>("InvoiceCode") + "\n";
                        text += "发票号码：" + words_result.Value<string>("InvoiceNum") + "\n";
                        text += "开票日期：" + words_result.Value<string>("InvoiceDate") + "\n";
                        text += "合计金额：" + words_result.Value<string>("TotalAmount") + "\n";
                        text += "合计税额：" + words_result.Value<string>("TotalTax") + "\n";
                        text += "价税合计(小写)：" + words_result.Value<string>("AmountInFiguers") + "\n";
                        text += "价税合计(大写)：" + words_result.Value<string>("AmountInWords") + "\n";
                        text += "校验码：" + words_result.Value<string>("CheckCode") + "\n";
                        text += "销售方名称：" + words_result.Value<string>("SellerName	") + "\n";
                        text += "销售方纳税人识别号：" + words_result.Value<string>("SellerRegisterNum") + "\n";
                        text += "购方名称：" + words_result.Value<string>("PurchaserName") + "\n";
                        text += "购方纳税人识别号：" + words_result.Value<string>("PurchaserRegisterNum") + "\n";
                        text += "货物名称：" + string.Join("；", words_result["CommodityName"].Select(word => word["word"].ToString())) + "\n"; //
                        text += "规格型号：" + string.Join("；", words_result["CommodityType"].Select(word => word["word"].ToString())) + "\n";
                        text += "数量：" +  string.Join("；", words_result["CommodityNum"].Select(word => word["word"].ToString())) + "\n";
                        text += "单价：" + string.Join("；", words_result["CommodityPrice"].Select(word => word["word"].ToString())) + "\n";
                        text += "金额：" + string.Join("；", words_result["CommodityAmount"].Select(word => word["word"].ToString())) + "\n";
                        text += "税率：" + string.Join("；", words_result["CommodityTaxRate"].Select(word => word["word"].ToString())) + "\n";
                        text += "税额：" + string.Join("；", words_result["CommodityTax"].Select(word => word["word"].ToString()));
                        //var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()));
                        HideCommandBar();
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            //VIN码Command
            VIN码Command = new RelayCommand(() =>
             {
                 try
                 {
                     var image = GetCutImageData();
                     if (image == null)
                     {
                         MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                         return;
                     }
                     var result = BaiduOcrClient.VinCode(image);
                     if (TextErrorHandler(result))
                     {
                         return;
                     }
                     var words_result_num = (int)result["words_result_num"];
                     if (words_result_num == 0)
                     {
                         MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                     }
                     else
                     {
                         var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()));
                         HideCommandBar();
                         ShowText(text);
                     }
                 }
                 catch (Exception e)
                 {
                     MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                 }
             });

            //高精度
            OcrAccurateCommand = new RelayCommand(() =>
            {
                ShowcallWrite();
            });
            //包含生僻字
            OcrGeneralEnhancedCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        return;
                    }

                    var result = BaiduOcrClient.GeneralEnhanced(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    var words_result_num = (int)result["words_result_num"];
                    if (words_result_num == 0)
                    {
                        MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else
                    {
                        var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()).ToArray());
                        HideCommandBar();
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
               
            });
            ////一键扫描
            //ScanningCommand = new RelayCommand(() =>
            //{
            //    try
            //    {
            //        ImageFile result = Scan();
            //        result.SaveFile(@"d:\1.png");
            //    }
            //    catch (Exception)
            //    {
            //        //Console.WriteLine(e.ToString());
            //        //throw;
            //        MessageBox.Show("找不到扫描仪！","系统提示！",MessageBoxButton.OK,MessageBoxImage.Error);

            //    }
            //    //Console.ReadKey();
            //});

            //银行卡
            OcrBankCardCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        return;
                    }

                    var result = BaiduOcrClient.Bankcard(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    string bank_card_type;
                    var bank_info = result["result"];
                    switch ((int)bank_info["bank_card_type"])
                    {
                        case 0:
                            bank_card_type = "不能识别";
                            break;
                        case 1:
                            bank_card_type = "借记卡";
                            break;
                        case 2:
                            bank_card_type = "信用卡";
                            break;
                        default:
                            bank_card_type = "数据异常";
                            break;
                    }
                    var text = string.Format("开户行：{0}\n卡号：{1}\n类型：{2}",
                        (string)bank_info["bank_name"],
                        (string)bank_info["bank_card_number"],
                        bank_card_type);

                    HideCommandBar();
                    ShowText(text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            });

            Func<JObject, bool, string> IdCardDisp = delegate (JObject result, bool isFront)
            {
                var text = isFront ? "身份证正面（图片状态：" : "身份证背面（图片状态：";
                switch ((string)result["image_status"])
                {
                    case "normal":
                        text += "识别正常";
                        break;
                    case "reversed_side":
                        text += "未摆正身份证";
                        break;
                    case "non_idcard":
                        text += "上传的图片中不包含身份证";
                        break;
                    case "blurred":
                        text += "身份证模糊";
                        break;
                    case "over_exposure":
                        text += "身份证关键字段反光或过曝";
                        break;
                    case "unknown":
                        text += "未知状态";
                        break;
                    default:
                        text += "无法识别的图片状态";
                        break;
                }
                text += "）\n----------------------------------------------\n";

                int words_result_num = (int)result["words_result_num"];
                if (words_result_num > 0)
                {
                    text += string.Join("\n", result["words_result"].Select(item =>
                    {
                        var line = item as JProperty;
                        return string.Format("{0}：{1}", line.Name, line.Value["words"]);
                    }).ToArray());
                }
                else
                {
                    text += "没有有效信息。";
                }
                return text;
            };

            //身份证
            OcrIdcardCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    string idCardSide= "front";
                    string idCardSideNext= "back";
                    if (image == null)
                    {
                        return;
                    }
                    // 身份证正面识别
                    var result1 = BaiduOcrClient.Idcard(image, idCardSide);
                    var result2 = BaiduOcrClient.Idcard(image, idCardSideNext);
                    var foundFornt = !TextErrorHandler(result1, false);
                    var fountBack = !TextErrorHandler(result2, false);
                    if (!foundFornt && !fountBack)
                    {
                        TextErrorHandler(result1);
                        return;
                    }

                    var text = "";
                    if (foundFornt)
                    {
                        text += IdCardDisp(result1, true);
                    }

                    if (fountBack)
                    {
                        if (foundFornt)
                        {
                            text += "\n\n";
                        }
                        text += IdCardDisp(result2, false);
                    }
                    HideCommandBar();
                    ShowText(text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            //驾驶证
            OcrDrivingLicenseCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        return;
                    }

                    var result = BaiduOcrClient.DrivingLicense(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    var text = "";
                    int words_result_num = (int)result["words_result_num"];
                    if (words_result_num > 0)
                    {
                        text += string.Join("\n", result["words_result"].Select(item =>
                        {
                            var line = item as JProperty;
                            return string.Format("{0}：{1}", line.Name, line.Value["words"]);
                        }).ToArray());
                    }
                    else
                    {
                        text += "没有有效信息。";
                    }

                    HideCommandBar();
                    ShowText(text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            });
            //行驶证
            OcrVehicleLicenseCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        return;
                    }

                    var result = BaiduOcrClient.VehicleLicense(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    var text = "";
                    int words_result_num = (int)result["words_result_num"];
                    if (words_result_num > 0)
                    {
                        text += string.Join("\n", result["words_result"].Select(item =>
                        {
                            var line = item as JProperty;
                            return string.Format("{0}：{1}", line.Name, line.Value["words"]);
                        }).ToArray());
                    }
                    else
                    {
                        text += "没有有效信息。";
                    }

                    HideCommandBar();
                    ShowText(text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            });
            //车牌号
            OcrPlateLicenseCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        return;
                    }

                    var result = BaiduOcrClient.LicensePlate(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    var text = string.Format("车牌号：{0}\n车牌颜色：{1}",
                        result["words_result"]["number"],
                        result["words_result"]["color"]);

                    HideCommandBar();
                    ShowText(text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            });
            //营业执照
            OcrBusinessLicenseCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        return;
                    }

                    var result = BaiduOcrClient.BusinessLicense(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    var text = "";
                    int words_result_num = (int)result["words_result_num"];
                    if (words_result_num > 0)
                    {
                        text += string.Join("\n", result["words_result"].Select(item =>
                        {
                            var line = item as JProperty;
                            return string.Format("{0}：{1}", line.Name, line.Value["words"]);
                        }).ToArray());
                    }
                    else
                    {
                        text += "没有有效信息。";
                    }

                    HideCommandBar();
                    ShowText(text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            });
            //普通票据
            OcrReceiptCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        return;
                    }
                    var options = new Dictionary<string, object>
                {
                    {"recognize_granularity", "small"}  // 定位单字符位置
                };
                    var result = BaiduOcrClient.Receipt(image);
                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    var text = "";
                    int words_result_num = (int)result["words_result_num"];
                    if (words_result_num > 0)
                    {
                        text += string.Join("\n", result["words_result"].Select(item => (string)item["words"]).ToArray());
                    }
                    else
                    {
                        text += "没有有效信息。";
                    }

                    HideCommandBar();
                    ShowText(text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            });

            // 识别为表格
            OcrFormCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImageData();
                    if (image == null)
                    {
                        return;
                    }

                    //var options = new Dictionary<string, object>
                    //{
                    //    {"recognize_granularity", "small"},  // 定位单字符位置
                    //};
                    // var result = BaiduOcrClient.TableRecognitionToExcel(image);
                    var result = BaiduOcrClient.TableRecognitionToJson(image, 30000);

                    if (TextErrorHandler(result))
                    {
                        return;
                    }

                    //Utils.HttpDownloadFile(result["result"].Value<string>(), "识别结果.xls");
                    //HideCommandBar();//隐藏文本框
                    //TableWindow tableWindow1 = new TableWindow(SelectArea, "识别结果.xls");
                    //tableWindow1.Show();
                    //Close();//关闭截屏的窗口
                    //return;


                    if (result["result"]["ret_msg"].Value<string>() != "已完成")
                    {
                        throw new Exception("识别未在规定时间内完成");
                    }

                    var raw_str = result["result"]["result_data"].Value<string>();
                    var result_json = (JObject)JsonConvert.DeserializeObject(raw_str);
                    var forms = result_json["forms"].Value<JArray>();

                    // 保存路径
                    string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string SoftwareName = "屏幕识文";
                    string LocalAppData = Path.Combine(LocalApplicationData, SoftwareName);

                    string historyDir = Path.Combine(LocalAppData, "history");
                    var xls_path = Path.Combine(historyDir, DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx");

                    using (var workbook = new XLWorkbook())
                    {
                        for (var i = 0; i < forms.Count; i++)
                        {
                            var form = forms[i];
                            var head = form["header"].Value<JArray>();
                            var foot = form["footer"].Value<JArray>();
                            var body = form["body"].Value<JArray>();

                            var worksheet = workbook.Worksheets.Add("Sheet" + (i + 1));

                            // 先填写body，因为可能会与head和foot交叉
                            ExcelHelper.BaiduAI_AddCells(worksheet, body);

                            ExcelHelper.BaiduAI_AddCells(worksheet, head, true);
                            ExcelHelper.BaiduAI_AddCells(worksheet, foot, true);
                        }

                        workbook.SaveAs(xls_path);
                    }

                    // Utils.HttpDownloadFile(id.ToString(), "识别结果.xls");
                    HideCommandBar();//隐藏文本框
                    TableWindow tableWindow = new TableWindow(SelectArea, xls_path);
                    tableWindow.Show();
                    Close();//关闭截屏的窗口
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            });
                        

            #endregion
            //二维码和条形码
            ScanCodeCommand = new RelayCommand(() =>
            {
                try
                {
                    var image = GetCutImage();
                    if (image == null)
                    {
                        return;
                    }
                    var reader = new BarcodeReader();
                    var result = reader.Decode(image);

                    if (result == null)
                    {
                        MessageBox.Show("没有找到任何码。", "识别结果", MessageBoxButton.OK);
                    }
                    else
                    {
                        var text = result.Text;
                        HideCommandBar();
                        ShowText(text);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                }
               
            });

            FullRectangle = new Rect(0, 0, Width, Height);

            DataContext = this;

            Snapshot();

            MouseLeftButtonDown += FullScreenWindow_MouseLeftButtonDown;
            MouseMove += FullScreenWindow_MouseMove;
            MouseLeftButtonUp += FullScreenWindow_MouseLeftButtonUp;

            TextResult.Visibility = Visibility.Hidden;
            TextResult.SizeChanged += TextResult_SizeChanged;

            CommandBar.Visibility = Visibility.Hidden;
            CommandBar.SizeChanged += CommandBar_SizeChanged;

            Closed += FullScreenWindow_Closed;

            InitBaiduAip();
        }

        //调用文字和精准度的方法
        public void ShowcallWrite()
        {
            try
            {
                var image = GetCutImageData();
                if (image == null)
                {
                    MessageBox.Show("图片为空！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var result = BaiduOcrClient.Accurate(image);
                if (TextErrorHandler(result))
                {
                    return;
                }
                var words_result_num = (int)result["words_result_num"];
                if (words_result_num == 0)
                {
                    MessageBox.Show("什么也没有识别出来，可能是空白图片。", "识别结果", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    var text = string.Join("\n", result["words_result"].Select(word => word["words"].ToString()).ToArray());
                    HideCommandBar();
                    ShowText(text);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("网络出现异常,请检查网络！", "系统提示！", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #region 简单错误处理
        public static Dictionary<int, string> ErrorMsgDisp = new Dictionary<int, string>
        {
            { 216015, "模块关闭" },
            { 216100, "非法参数" },
            { 216101, "参数数量不够" },
            { 216102, "业务不支持" },
            { 216103, "参数太长" },
            { 216110, "APP ID不存在" },
            { 216111, "非法用户ID" },
            { 216200, "图片为空，请检查后重新尝试" },
            { 216201, "图片格式错误" },
            { 216202, "图片大小错误" },
            { 216300, "DB错误" },
            { 216400, "后端系统错误" },
            { 216401, "内部错误" },
            { 216500, "未知错误" },
            { 216600, "身份证的ID格式错误" },
            { 216601, "身份证的ID和名字不匹配" },
            { 216630, "识别错误" },
            { 216631, "识别银行卡错误" },
            { 216632, "unknown error" },
            { 216633, "识别身份证错误" },
            { 216634, "检测错误" },
            { 216635, "获取mask图片错误" },
            { 282000, "业务逻辑层内部错误" },
            { 282001, "业务逻辑层后端服务错误" },
            { 282002, "请求参数编码错误" },
            { 282100, "图片压缩转码错误" },
            { 282102, "未检测到图片中识别目标，请确保图片中包含对应卡证票据" },
            { 282110, "参数不存在" },
            { 282111, "URL格式非法" },
            { 282112, "URL下载超时" },
            { 282113, "URL返回无效数据" },
            { 282114, "URL长度超过1024字节或为0" },
            { 282809, "返回结果请求错误" },
            { 282810, "图像识别错误" }
        };

        private bool TextErrorHandler(JObject result, bool handleIt = true)
        {
            JToken error_code_token;
            if (result.TryGetValue("error_code", out error_code_token))
            {
                if (handleIt)
                {
                    int error_code = (int)error_code_token;
                    var error_disp = ErrorMsgDisp.ContainsKey(error_code) ?
                        string.Format("错误码：{0}\n描述：{1}", error_code, ErrorMsgDisp[error_code]) :
                      "识别出现异常错误！" ;
                    MessageBox.Show(error_disp, "识别失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 命令条的显示
        private double CommandBarWidth;
        private double CommandBarHeight;
        private void CommandBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CommandBarWidth = e.NewSize.Width;
            CommandBarHeight = e.NewSize.Height;
        }
        private void ShowCommandBar()
        {
            CommandBar.Visibility = Visibility.Visible;

            // 看看左侧的空间是否足够，如果不够，那就靠左对齐
            if (SelectRectangle.Right < CommandBarWidth)
            {
                CommandBar.SetValue(Canvas.LeftProperty, SelectRectangle.Left);
            }
            else
            {
                CommandBar.SetValue(Canvas.LeftProperty, SelectRectangle.Right - CommandBarWidth);
            }

            double bottomHeight = Height - SelectRectangle.Bottom;
            double topHeight = SelectRectangle.Top;

            if (bottomHeight >= CommandBarHeight + 2)
            {
                // 位于下方
                CommandBar.SetValue(Canvas.TopProperty, SelectRectangle.Bottom);
            }
            else if (topHeight >= CommandBarHeight + 2)
            {
                // 位于上方
                CommandBar.SetValue(Canvas.TopProperty, SelectRectangle.Top - 4 - CommandBarHeight);
            }
            else
            {
                // 位于内部右上角
                CommandBar.SetValue(Canvas.TopProperty, SelectRectangle.Top - 2);
            }
        }

        private void HideCommandBar()
        {
            if (CommandBar.Visibility == Visibility.Visible)
            {
                CommandBar.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region OCR操作辅助函数
        private Baidu.Aip.Ocr.Ocr BaiduOcrClient;
        private void InitBaiduAip()
        {
            var apiKey = "-- 填入百度 API-KEY --";
            var secretKey = "-- 填入百度 SECRET-KEY --";
            BaiduOcrClient = new Baidu.Aip.Ocr.Ocr(apiKey, secretKey);
        }

        //获取用户截取的图片
        private Bitmap GetCutImage()
        {
            var cutArea = new System.Drawing.Rectangle((int)SelectRectangle.X, (int)SelectRectangle.Y, (int)SelectRectangle.Width, (int)SelectRectangle.Height);
            if (cutArea.Width <= 0 || cutArea.Height <= 0)
            {
                return null;
            }
            else
            {
                Bitmap selectArea = new Bitmap(cutArea.Width, cutArea.Height);
                Graphics g = Graphics.FromImage(selectArea);
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.SmoothingMode = SmoothingMode.HighQuality;

                g.RotateTransform(-(float)Degree);
                var centerX = SelectRectangle.Left + SelectRectangle.Width / 2;
                var centerY = SelectRectangle.Top + SelectRectangle.Height / 2;
                var angle = Degree / 180 * Math.PI;
                var x = -SelectRectangle.Width / 2;
                var y = -SelectRectangle.Height / 2;
                var rotateX = x * Math.Cos(-angle) + y * Math.Sin(-angle);
                var rotateY = y * Math.Cos(-angle) - x * Math.Sin(-angle);

                var dx = (float)(centerX + rotateX);
                var dy = (float)(centerY + rotateY);
                g.TranslateTransform(-dx, -dy);
                g.DrawImageUnscaled(ScreenSnapshot, 0, 0);
                g.Dispose();

                // 记录一下截取到的图片
                SelectArea = selectArea;

                return SelectArea;
            }
        }

        private Bitmap SelectArea;
        private byte[] GetCutImageData(string imageFormat = "png")
        {
            GetCutImage();
            if (SelectArea != null) {
                var stream = new MemoryStream();
                switch (imageFormat.ToLower())
                {
                    case "png":
                    default:
                        SelectArea.Save(stream, ImageFormat.Png);
                        break;
                    case "jpg":
                        SelectArea.Save(stream, ImageFormat.Jpeg);
                        break;
                }
                var image = stream.ToArray();
                Debug.WriteLine("图片格式：" + imageFormat + "    数据尺寸：" + image.Count() + "字节");
                return image;
            }
            else
            {
                return null;
            }
        }
        #endregion
       

        public void FullScreenWindow_Closed(object sender, EventArgs e)
        {   }

        #region 鼠标操作
        //private bool isDrawing = false; // 是否在进行框选操作
        //private bool mouseMoved = false;
        // private bool isOverSelectedArea = false; // 鼠标所在位置是否位于选择框内
        // private bool isMoving = false; // 是否在进行移动操作
        private System.Windows.Point mouseStartPosition;
        private bool IsMouseDown = false;//初始鼠标的状态为false 
        private void FullScreenWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 鼠标状态
            IsMouseDown = true;
            startBox = SelectRectangle;
            mouseStartPosition = e.GetPosition(this); // 鼠标落下的位置
            MouseStartPos2 = mouseStartPosition;
            startDegree = Degree; // 选择的角度

            HideCommandBar(); // 隐藏控制条

            if (HitWhitch == HitWhitchTypes.none)
            {
                Degree = 0;
            }

            if (HitWhitch == HitWhitchTypes.none || HitWhitch == HitWhitchTypes.rect_box)
            {
                path.Visibility = Visibility.Collapsed;//隐藏path
            }
        }

        // 整个屏幕的区域
        public Rect FullRectangle { get; set; }

        // 用户选择的区域
        public Rect SelectRectangle { get; set; }
        public Bitmap ImageSavePath { get; private set; }

        private HitWhitchTypes HitWhitch;//定义对象

        private System.Windows.Point MouseStartPos2;
        private Rect startBox;
        private double startDegree;//开始的度数
        private const double RectSize = 6;//八个小盒子的大小
        private double Degree = 0;//结束的度数
        private Cursor RotateCursor=Cursors.SizeAll;//定义变量鼠标

        //定义11种击中的状态，初始化为none
        public enum HitWhitchTypes
        {
            none,
            rect_left_top1,
            rect_top2,
            rect_right_top3,
            rect_right4,
            rect_right_bottom5,
            rect_bottom6,
            rect_left_bottom7,
            rect_left8,
            line,
            line1,
            line2,
            line3,
            line4,
            box_rotate,
            rect_box
        };
        
       
        //以选中的区域左上角为坐标原点进行设置四条边之间的变化
        private static bool RectContains(System.Windows.Point p, double left, double right, double top, double bottom)
        {
            return p.X >= left && p.X <= right && p.Y >= top && p.Y <= bottom;
        }
       
        //鼠标移动计算鼠标的移动的位置
        private void FullScreenWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown)//(判断一下鼠标是否在用户选中的区域按下)
            {
                var box = SelectRectangle;
                //获取面板的鼠标位置
                var p2 = e.GetPosition(canvas);
                var v2 = p2 - MouseStartPos2;

                //旋转的角度
                var angle = -startDegree / 180.0 * Math.PI;
                var angle_back = -angle;

                //定义两个变量计算cos和sin
                var cos_angle = Math.Cos(angle);
                var sin_angle = Math.Sin(angle);

                var rotateBackV = new System.Windows.Vector(
                    v2.X * Math.Cos(angle_back) + v2.Y * Math.Sin(angle_back),
                    v2.Y * Math.Cos(angle_back) - v2.X * Math.Sin(angle_back)
                );
                
                switch (HitWhitch)
                {
                    case HitWhitchTypes.none://击中灰色区域
                        {
                            // mouseMoved = true;
                            var currMousePosition = e.GetPosition(this);
                            var x = (int)Math.Min(mouseStartPosition.X, currMousePosition.X);
                            var y = (int)Math.Min(mouseStartPosition.Y, currMousePosition.Y);
                            var w = (int)Math.Abs(mouseStartPosition.X - currMousePosition.X);
                            var h = (int)Math.Abs(mouseStartPosition.Y - currMousePosition.Y);


                            SelectRectangle = new Rect(x, y, w, h);
                            RaisePropertyChanged(nameof(SelectRectangle));//这句代码表示鼠标跟随作用
                            DrawRects();
                        }
                        break;

                    case HitWhitchTypes.rect_box://击中选中区域
                        {
                            // mouseMoved = true;
                            var currMousePosition = e.GetPosition(this);
                            var diff = currMousePosition - mouseStartPosition;

                            // 这部分代码用于保证选择框不会超出屏幕边界。
                            var minDiffX = 0 - startBox.Left;
                            var maxDiffX = ActualWidth - startBox.Right;
                            var minDiffY = 0 - startBox.Top;
                            var maxDiffY = ActualHeight - startBox.Bottom;
                            diff.X = Math.Min(Math.Max(diff.X, minDiffX), maxDiffX);
                            diff.Y = Math.Min(Math.Max(diff.Y, minDiffY), maxDiffY);

                            var rect = startBox;
                            rect.Offset(diff);
                            SelectRectangle = rect;
                            RaisePropertyChanged(nameof(SelectRectangle));
                            DrawRects();
                        }
                        break;
                    case HitWhitchTypes.rect_left_top1://击中左上角的盒子
                        {
                            //获取新的位置宽和高
                            var newWidth = startBox.Width - rotateBackV.X;
                            var newHeight = startBox.Height - rotateBackV.Y;

                            //用数学公式换算新的X轴和Y轴
                            var newX = startBox.Left + startBox.Width / 2 * (1 + cos_angle) + startBox.Height / 2 * sin_angle
                                - (newWidth / 2 * (1 + cos_angle) + newHeight / 2 * sin_angle);
                            var newY = startBox.Top + startBox.Height / 2 * (1 + cos_angle) - startBox.Width / 2 * sin_angle
                                - (newHeight / 2 * (1 + cos_angle) - newWidth / 2 * sin_angle);                           

                            //判断新的位置
                            if (newWidth < 0)
                            {
                                box. X= newX + newWidth;
                                box.Width = -newWidth;
                            }
                            else
                            {
                                box.X = newX;
                                box.Width = newWidth;
                            }

                            if (newHeight < 0)
                            {
                                box.Y = newY + newHeight;
                                box.Height = -newHeight;
                            }
                            else
                            {
                                box.Y = newY;
                                box.Height = newHeight;
                            }
                            SelectRectangle = box;//赋值
                            RaisePropertyChanged(nameof(SelectRectangle));//自动更新跟随
                            DrawRects();
                        }
                        break;
                    case HitWhitchTypes.rect_top2://击中上边中间
                        {
                            //获取新的位置宽和高
                            var newWidth = startBox.Width;
                            var newHeight = startBox.Height - rotateBackV.Y;
                            //定义变量获取开始的位置
                            var startLeftBottomV = new System.Windows.Vector(-startBox.Width / 2, startBox.Height / 2);
                            //定义变量获取旋转以后的位置（用数学公式）
                            var startRotatedLeftBottomV = new System.Windows.Vector(
                                startLeftBottomV.X * cos_angle + startLeftBottomV.Y * sin_angle,
                                startLeftBottomV.Y * cos_angle - startLeftBottomV.X * sin_angle);
                            //定义变量获取开始的中心位置
                            var startCenter = new System.Windows.Vector(startBox.Left + startBox.Width / 2,
                                startBox.Top + startBox.Height / 2);
                            //定义变量获取新的位置
                            var newLeftBottomV = new System.Windows.Vector(
                                -newWidth / 2, newHeight / 2);
                            //定义变量获取旋转后的位置
                            var newRotatedLeftBottomV = new System.Windows.Vector(
                                newLeftBottomV.X * cos_angle + newLeftBottomV.Y * sin_angle,
                                newLeftBottomV.Y * cos_angle - newLeftBottomV.X * sin_angle);
                            //最新的中心位置=开始的中心位置+开始旋转的位置-新的旋转后的位置
                            var newCenter = startCenter + startRotatedLeftBottomV - newRotatedLeftBottomV;
                            var newLeft = newCenter.X - newWidth / 2;//定义变量获取新的左边的距离
                            var newTop = newCenter.Y - newHeight / 2;//定义变量获取上边的离盒子的距离

                            if (newWidth < 0)
                            {
                                box.X = newLeft + newWidth;
                                box.Width = -newWidth;
                            }
                            else
                            {
                                box.X = newLeft;
                                box.Width = newWidth;
                            }

                            if (newHeight < 0)
                            {
                                box.Y = newTop + newHeight;
                                box.Height = -newHeight;
                            }
                            else
                            {
                                box.Y = newTop;
                                box.Height = newHeight;
                            }

                            SelectRectangle = box;//赋值
                            RaisePropertyChanged(nameof(SelectRectangle));//自动更新跟随
                            DrawRects();
                        }
                        break;
                    case HitWhitchTypes.rect_right_top3://击中右上角
                        {
                            //获取新的位置宽和高
                            var newWidth = startBox.Width + rotateBackV.X;
                            var newHeight = startBox.Height - rotateBackV.Y;

                            var startLeftBottomV = new System.Windows.Vector(-startBox.Width / 2, startBox.Height / 2);
                            var startRotatedLeftBottomV = new System.Windows.Vector(
                                startLeftBottomV.X * cos_angle + startLeftBottomV.Y * sin_angle,
                                startLeftBottomV.Y * cos_angle - startLeftBottomV.X * sin_angle);
                            var startCenter = new System.Windows.Vector(startBox.Left + startBox.Width / 2,
                                startBox.Top + startBox.Height / 2);

                            var newLeftBottomV = new System.Windows.Vector(
                                -newWidth / 2, newHeight / 2);
                            var newRotatedLeftBottomV = new System.Windows.Vector(
                                newLeftBottomV.X * cos_angle + newLeftBottomV.Y * sin_angle,
                                newLeftBottomV.Y * cos_angle - newLeftBottomV.X * sin_angle);

                            var newCenter = startCenter + startRotatedLeftBottomV - newRotatedLeftBottomV;
                            var newLeft = newCenter.X - newWidth / 2;
                            var newTop = newCenter.Y - newHeight / 2;

                            if (newWidth < 0)
                            {
                                box.X = newLeft + newWidth;
                                box.Width = -newWidth;
                            }
                            else
                            {
                                box.X = newLeft;
                                box.Width = newWidth;
                            }

                            if (newHeight < 0)
                            {
                                box.Y = newTop + newHeight;
                                box.Height = -newHeight;
                            }
                            else
                            {
                                box.Y = newTop;
                                box.Height = newHeight;
                            }
                            SelectRectangle = box;//赋值
                            RaisePropertyChanged(nameof(SelectRectangle));//自动更新跟随
                            DrawRects();
                        }
                        break;
                    case HitWhitchTypes.rect_right4:
                        {
                            //获取新的位置宽和高
                            var newWidth = startBox.Width + rotateBackV.X;
                            var newHeight = startBox.Height;

                            var startLeftBottomV = new System.Windows.Vector(-startBox.Width / 2, startBox.Height / 2);
                            var startRotatedLeftBottomV = new System.Windows.Vector(
                                startLeftBottomV.X * cos_angle + startLeftBottomV.Y * sin_angle,
                                startLeftBottomV.Y * cos_angle - startLeftBottomV.X * sin_angle);
                            var startCenter = new System.Windows.Vector(startBox.Left + startBox.Width / 2,
                                startBox.Top + startBox.Height / 2);

                            var newLeftBottomV = new System.Windows.Vector(
                                -newWidth / 2, newHeight / 2);
                            var newRotatedLeftBottomV = new System.Windows.Vector(
                                newLeftBottomV.X * cos_angle + newLeftBottomV.Y * sin_angle,
                                newLeftBottomV.Y * cos_angle - newLeftBottomV.X * sin_angle);

                            var newCenter = startCenter + startRotatedLeftBottomV - newRotatedLeftBottomV;
                            var newLeft = newCenter.X - newWidth / 2;
                            var newTop = newCenter.Y - newHeight / 2;

                            if (newWidth < 0)
                            {
                                box.X = newLeft + newWidth;
                                box.Width = -newWidth;
                            }
                            else
                            {
                                box.X = newLeft;
                                box.Width = newWidth;
                            }

                            if (newHeight < 0)
                            {
                                box.Y = newTop + newHeight;
                                box.Height = -newHeight;
                            }
                            else
                            {
                                box.Y = newTop;
                                box.Height = newHeight;
                            }
                            DrawRects();
                            SelectRectangle = box;//赋值
                            RaisePropertyChanged(nameof(SelectRectangle));//自动更新跟随
                        }
                        break;
                    case HitWhitchTypes.rect_right_bottom5:
                        {
                            //获取新的位置宽和高
                            var newWidth = startBox.Width + rotateBackV.X;
                            var newHeight = startBox.Height + rotateBackV.Y;
                            //定义变量左上角不动
                            var startTopleft = new System.Windows.Vector(-startBox.Width / 2, -startBox.Height / 2);
                            //旋转后的左上角位置
                            var startRotatedTopleft = new System.Windows.Vector(
                                startTopleft.X * cos_angle + startTopleft.Y * sin_angle,
                                startTopleft.Y * cos_angle - startTopleft.X * sin_angle);
                            //开始的中心位
                            var startCenter = new System.Windows.Vector(startBox.Left + startBox.Width / 2,
                               startBox.Top + startBox.Height / 2);
                            //新的左上角位置
                            var newLeftTopleft = new System.Windows.Vector(
                               -newWidth / 2, -newHeight / 2);
                            //新的旋转后的新位置
                            var newRotatedTopleft = new System.Windows.Vector(
                                newLeftTopleft.X * cos_angle + newLeftTopleft.Y * sin_angle,
                                newLeftTopleft.Y * cos_angle - newLeftTopleft.X * sin_angle);
                            //新的中心位置
                            var newCenter = startCenter + startRotatedTopleft - newRotatedTopleft;
                            var newLeft = newCenter.X - newWidth / 2;
                            var newTop = newCenter.Y - newHeight / 2;

                            if (newWidth < 0)
                            {
                                box.X = newLeft + newWidth;
                                box.Width = -newWidth;
                            }
                            else
                            {
                                box.X = newLeft;
                                box.Width = newWidth;
                            }

                            if (newHeight < 0)
                            {
                                box.Y = newTop + newHeight;
                                box.Height = -newHeight;
                            }
                            else
                            {
                                box.Y = newTop;
                                box.Height = newHeight;
                            }
                            SelectRectangle = box;//赋值
                            RaisePropertyChanged(nameof(SelectRectangle));//自动更新跟随
                            DrawRects();
                        }
                        break;
                    case HitWhitchTypes.rect_bottom6:
                        {
                            //获取新的位置宽和高
                            var newWidth = startBox.Width;
                            var newHeight = startBox.Height + rotateBackV.Y;
                            //定义变量左上角不动
                            var startTopleft = new System.Windows.Vector(-startBox.Width / 2, -startBox.Height / 2);
                            //旋转后的左上角位置
                            var startRotatedTopleft = new System.Windows.Vector(
                                startTopleft.X * cos_angle + startTopleft.Y * sin_angle,
                                startTopleft.Y * cos_angle - startTopleft.X * sin_angle);
                            //开始的中心位
                            var startCenter = new System.Windows.Vector(startBox.Left + startBox.Width / 2,
                               startBox.Top + startBox.Height / 2);
                            //新的左上角位置
                            var newLeftTopleft = new System.Windows.Vector(
                               -newWidth / 2, -newHeight / 2);
                            //新的旋转后的新位置
                            var newRotatedTopleft = new System.Windows.Vector(
                                newLeftTopleft.X * cos_angle + newLeftTopleft.Y * sin_angle,
                                newLeftTopleft.Y * cos_angle - newLeftTopleft.X * sin_angle);
                            //新的中心位置
                            var newCenter = startCenter + startRotatedTopleft - newRotatedTopleft;
                            var newLeft = newCenter.X - newWidth / 2;
                            var newTop = newCenter.Y - newHeight / 2;

                            if (newWidth < 0)
                            {
                                box.X = newLeft + newWidth;
                                box.Width = -newWidth;
                            }
                            else
                            {
                                box.X = newLeft;
                                box.Width = newWidth;
                            }

                            if (newHeight < 0)
                            {
                                box.Y = newTop + newHeight;
                                box.Height = -newHeight;
                            }
                            else
                            {
                                box.Y = newTop;
                                box.Height = newHeight;
                            }
                            SelectRectangle = box;//赋值
                            RaisePropertyChanged(nameof(SelectRectangle));//自动更新跟随
                            DrawRects();
                        }
                        break;
                    case HitWhitchTypes.rect_left_bottom7:
                        {
                            //获取左下角的位置宽和高
                            var newWidth = startBox.Width - rotateBackV.X;
                            var newHeight = startBox.Height + rotateBackV.Y;
                            //定义变量右上角不动
                            var startTopright = new System.Windows.Vector(startBox.Width / 2, -startBox.Height / 2);
                            //开始的右上角位置
                            //RotateVector();
                            var startRotatedTopright = new System.Windows.Vector(
                                startTopright.X * cos_angle + startTopright.Y * sin_angle,
                                startTopright.Y * cos_angle - startTopright.X * sin_angle);
                            //开始的中心位置
                            var startCenter = new System.Windows.Vector(startBox.Left + startBox.Width / 2,
                               startBox.Top + startBox.Height / 2);
                            //新的右上角位置
                            var newLeftTopright = new System.Windows.Vector(
                               newWidth / 2, -newHeight / 2);
                            //新的旋转后的新位置
                            var newRotatedTopright = new System.Windows.Vector(
                                newLeftTopright.X * cos_angle + newLeftTopright.Y * sin_angle,
                                newLeftTopright.Y * cos_angle - newLeftTopright.X * sin_angle);
                            //新的中心位置
                            var newCenter = startCenter + startRotatedTopright - newRotatedTopright;
                            var newLeft = newCenter.X - newWidth / 2;
                            var newTop = newCenter.Y - newHeight / 2;

                            if (newWidth < 0)
                            {
                                box.X = newLeft + newWidth;
                                box.Width = -newWidth;
                            }
                            else
                            {
                                box.X = newLeft;
                                box.Width = newWidth;
                            }

                            if (newHeight < 0)
                            {
                                box.Y = newTop + newHeight;
                                box.Height = -newHeight;
                            }
                            else
                            {
                                box.Y = newTop;
                                box.Height = newHeight;
                            }
                            SelectRectangle = box;//赋值
                            RaisePropertyChanged(nameof(SelectRectangle));//自动更新跟随
                            DrawRects();
                        }
                        break;
                    case HitWhitchTypes.rect_left8:
                        {
                            //获取新的位置宽和高
                            var newWidth = startBox.Width - rotateBackV.X;
                            var newHeight = startBox.Height;

                            //换算新的X轴和Y轴
                            var newX = startBox.Left + startBox.Width / 2 * (1 + cos_angle) + startBox.Height / 2 * sin_angle
                                - (newWidth / 2 * (1 + cos_angle) + newHeight / 2 * sin_angle);
                            var newY = startBox.Top + startBox.Height / 2 * (1 + cos_angle) - startBox.Width / 2 * sin_angle
                                - (newHeight / 2 * (1 + cos_angle) - newWidth / 2 * sin_angle);

                            if (newWidth < 0)
                            {
                                box.X = newX + newWidth;
                                box.Width = -newWidth;
                            }
                            else
                            {
                                box.X = newX;
                                box.Width = newWidth;
                            }

                            if (newHeight < 0)
                            {
                                box.Y = newY + newHeight;
                                box.Height = -newHeight;
                            }
                            else
                            {
                                box.Y = newY;
                                box.Height = newHeight;
                            }
                            SelectRectangle = box;//赋值
                            RaisePropertyChanged(nameof(SelectRectangle));//自动更新跟随
                            DrawRects();
                        }
                        break;                   
                    case HitWhitchTypes.box_rotate://选中旋转的圆圈
                        {
                            var v1 = new System.Windows.Vector(0, -30 - startBox.Height / 2);
                            var v3 = v1 + rotateBackV;
                            var diffAngle = Math.Atan2(v3.X, -v3.Y);
                            var diffDegree = diffAngle / Math.PI * 180;
                            Degree = startDegree + diffDegree;
                            SelectRectangle = box;//赋值
                            RaisePropertyChanged(nameof(SelectRectangle));//自动更新跟随
                            DrawRects();
                        }
                        break;
                }
                
            }
            else
            {
                var oldHitWitch = HitWhitch;
                var p = e.GetPosition(path);
                if (path.Visibility != Visibility.Visible)//当鼠标移动的时候，点击的是灰色的面板
                {
                    HitWhitch = HitWhitchTypes.none;
                }
                // 左上角
                else if (rect_left_top1.Rect.Contains(p))
                {
                    HitWhitch = HitWhitchTypes.rect_left_top1;
                }
                // 上侧边
                else if (RectContains(p, 0, SelectRectangle.Width, - RectSize / 2, RectSize / 2))
                {
                    HitWhitch = HitWhitchTypes.rect_top2;
                }//右上角的变化
                else if (rect_right_top3.Rect.Contains(p))
                {
                    HitWhitch = HitWhitchTypes.rect_right_top3;
                }//右侧边
                else if (RectContains(p, SelectRectangle.Width - RectSize / 2, SelectRectangle.Width + RectSize/2, 0, SelectRectangle.Height))
                {
                    HitWhitch = HitWhitchTypes.rect_right4;
                }//右下角
                else if (rect_right_bottom5.Rect.Contains(p))
                {
                    HitWhitch = HitWhitchTypes.rect_right_bottom5;
                }//底侧边
                else if (RectContains(p, 0, SelectRectangle.Width, SelectRectangle.Height - RectSize / 2, SelectRectangle.Height + RectSize / 2))
                {
                    HitWhitch = HitWhitchTypes.rect_bottom6;
                }//左下角
                else if (rect_left_bottom7.Rect.Contains(p))
                {
                    HitWhitch = HitWhitchTypes.rect_left_bottom7;
                }//左侧边
                else if (RectContains(p,-RectSize/2,RectSize/2,0, SelectRectangle.Height))
                {
                    HitWhitch = HitWhitchTypes.rect_left8;
                }               
                //点击旋转
                else if ((ellips.Center - p).Length <= ellips.RadiusX)
                {
                    HitWhitch = HitWhitchTypes.box_rotate;
                }
                //击中盒子中间截取区域
                else if (p.X>=0&&p.X<=SelectRectangle.Width && p.Y>=0 && p.Y<= SelectRectangle.Height)
                {
                    HitWhitch = HitWhitchTypes.rect_box;
                }//什么都没有击中
                else
                {
                    HitWhitch = HitWhitchTypes.none;
                }
                
                if (oldHitWitch == HitWhitch)
                {
                    return;
                }
                switch (HitWhitch)//击中的区域
                {
                    case HitWhitchTypes.none:
                        Cursor = Cursors.Arrow;
                        break;
                    case HitWhitchTypes.rect_left_top1:
                        Cursor = Cursors.SizeNWSE;
                        break;
                    case HitWhitchTypes.rect_top2:
                        Cursor = Cursors.SizeNS;
                        break;
                    case HitWhitchTypes.rect_right_top3:
                        Cursor = Cursors.SizeNESW;
                        break;
                    case HitWhitchTypes.rect_right4:
                        Cursor = Cursors.SizeWE;
                        break;
                    case HitWhitchTypes.rect_right_bottom5:
                        Cursor = Cursors.SizeNWSE;
                        break;
                    case HitWhitchTypes.rect_bottom6:
                        Cursor = Cursors.SizeNS;
                        break;
                    case HitWhitchTypes.rect_left_bottom7:
                        Cursor = Cursors.SizeNESW;
                        break;
                    case HitWhitchTypes.rect_left8:
                        Cursor = Cursors.SizeWE;
                        break;
                    case HitWhitchTypes.rect_box:
                        Cursor = Cursors.SizeAll;
                        break;
                    case HitWhitchTypes.line1:
                        Cursor =Cursors.SizeWE;
                        break;
                    case HitWhitchTypes.line2:
                        Cursor = Cursors.SizeNS;
                        break;
                    case HitWhitchTypes.line3:
                        Cursor = Cursors.SizeWE;
                        break;
                    case HitWhitchTypes.line4:
                        Cursor = Cursors.SizeNS;
                        break;
                    case HitWhitchTypes.line:
                        Cursor = Cursors.SizeAll;
                        break;
                }
            }
        }

        public void DrawRects()//鼠标初始化位置
        {
            //路径面板的位置等于选中区域的位置
            //path.SetValue(Canvas.LeftProperty, SelectRectangle.Left);
            //path.SetValue(Canvas.TopProperty, SelectRectangle.Top);

            //旋转的初始位置
            box_rotate.CenterX = SelectRectangle.Width / 2;
            box_rotate.CenterY = SelectRectangle.Height / 2;
            box_rotate.Angle = Degree;

            SelectRectangleRotation.CenterX = SelectRectangle.Left + SelectRectangle.Width / 2;
            SelectRectangleRotation.CenterY = SelectRectangle.Top + SelectRectangle.Height / 2;
            SelectRectangleRotation.Angle = Degree;

            line1.StartPoint = new System.Windows.Point(0,0);
            line1.EndPoint = new System.Windows.Point(0,SelectRectangle.Height);

            line2.StartPoint = new System.Windows.Point(0, 0);
            line2.EndPoint = new System.Windows.Point(SelectRectangle.Width, 0 );

            line3.StartPoint = new System.Windows.Point(SelectRectangle.Width, 0);
            line3.EndPoint = new System.Windows.Point(SelectRectangle.Width, SelectRectangle.Height);

            line4.StartPoint = new System.Windows.Point(0, SelectRectangle.Height);
            line4.EndPoint = new System.Windows.Point(SelectRectangle.Width, SelectRectangle.Height);

            rect_left_top1.Rect = new Rect(-RectSize / 2, -RectSize / 2, RectSize, RectSize);

            rect_top2.Rect = new Rect(SelectRectangle.Width / 2 - RectSize / 2, -RectSize / 2, RectSize, RectSize);

            rect_right_top3.Rect = new Rect(SelectRectangle.Width - RectSize / 2, -RectSize / 2, RectSize, RectSize);

            rect_right4.Rect = new Rect(SelectRectangle.Width - RectSize / 2, SelectRectangle.Height / 2 - RectSize / 2, RectSize, RectSize);

            rect_right_bottom5.Rect = new Rect(SelectRectangle.Width - RectSize / 2, SelectRectangle.Height - RectSize / 2, RectSize, RectSize);

            rect_bottom6.Rect = new Rect(SelectRectangle.Width / 2 - RectSize / 2, SelectRectangle.Height - RectSize / 2, RectSize, RectSize);

            rect_left_bottom7.Rect = new Rect(-RectSize / 2, SelectRectangle.Height - RectSize / 2, RectSize, RectSize);

            rect_left8.Rect = new Rect(-RectSize / 2, SelectRectangle.Height / 2 - RectSize / 2, RectSize, RectSize);

        

            //旋转
            //circular.Rect = new Rect(SelectRectangle.Width / 2 - RectSize / 2, -40 - RectSize / 2, RectSize, RectSize);

            //八个盒子的大小
            // rect_box.Rect = new Rect(0, 0, SelectRectangle.Width, SelectRectangle.Height);
            //连接第二个和旋转的直线
            line.StartPoint = new System.Windows.Point(SelectRectangle.Width / 2, -RectSize / 2);
            line.EndPoint = new System.Windows.Point(SelectRectangle.Width / 2, -30 + RectSize / 2);

            ellips.Center = new System.Windows.Point(SelectRectangle.Width/2,-30);
            
        }

        //鼠标抬起结束
        private void FullScreenWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = false;//鼠标抬起的时候，判断鼠标为false
            ShowCommandBar();
             //设置新的面板
            DrawRects();
            //把隐藏的path显示出来
            path.Visibility = Visibility.Visible;
        }
        //鼠标取消命令
        private void UnselectArea()
        {
            SelectRectangle = Rect.Empty;
            RaisePropertyChanged(nameof(SelectRectangle));
            HideCommandBar();
            path.Visibility = Visibility.Collapsed;//解决bug边框显示问题
        }
        #endregion

        #region 文本框的显示
        private bool toSetRect = false;
        private const double TextBoxMinWidth = 100;
        private const double TextBoxMinHeight = 100;

        private void ShowText(string text)
        {
            // 显示识别结果
            var resultWindow = new ResultWindow(SelectArea, text);
            resultWindow.Show();
            Close();
        }

        // 调整位置，以实现跟随的效果
        // 调整尺寸，使内容全部可见
        private void TextResult_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!toSetRect)
            {
                return;
            }
            toSetRect = false;

            var bottomRoom = new System.Windows.Size(SelectRectangle.Right, Height - SelectRectangle.Bottom);
            var rightRoom = new System.Windows.Size(Width - SelectRectangle.Right, Height - SelectRectangle.Top);
            var topRoom = new System.Windows.Size(SelectRectangle.Right, SelectRectangle.Top);
            var leftRoom = new System.Windows.Size(SelectRectangle.Left, Height - SelectRectangle.Top);

            double left = Width * 0.25;
            double top = Height * 0.25;
            double width = Width * 0.5;
            double height = Height * 0.5;
            double fullWidth = e.NewSize.Width;
            double fullHeight = e.NewSize.Height;

            // IF 文本框尺寸小于空余尺寸
            //     ** 直接放置
            // ELSE （即文本框尺寸大于空余尺寸）
            //     IF 空余尺寸大于 100（40）
            //         ** 缩小文本框尺寸
            //            -> 这里的最小尺寸是用于判断是否缩小使用的，因为小于这个值就不太美观了（除非文本内容本来就很少）。
            //            -> 具体缩小至的尺寸，是全部可利用尺寸。
            //     ELSE （即空余尺寸小于 100（40）
            //         ** 将文本框放到截图上面去

            // 关于空间，我希望文本框与屏幕边界有间隔，这是为了美观。
            // 当然，这不包括与选择框对齐的那一条边。这也是为了美观。
            int widthFlag;
            int heightFlag;
            int flag;
            if (bottomRoom.Width >= fullWidth + 2)
            {
                widthFlag = 0;
            }
            else if (bottomRoom.Width >= TextBoxMinWidth + 2)
            {
                widthFlag = 1;
            }
            else
            {
                widthFlag = 2;
            }

            if (bottomRoom.Height >= fullHeight + 4)
            {
                heightFlag = 0;
            }
            else if (bottomRoom.Height >= TextBoxMinHeight + 4)
            {
                heightFlag = 1;
            }
            else
            {
                heightFlag = 2;
            }

            if (widthFlag != 2 && heightFlag != 2)
            {
                flag = 0;
            }
            else
            {
                if (rightRoom.Width >= fullWidth + 4)
                {
                    widthFlag = 0;
                }
                else if (rightRoom.Width >= TextBoxMinWidth + 4)
                {
                    widthFlag = 1;
                }
                else
                {
                    widthFlag = 2;
                }

                if (rightRoom.Height >= fullHeight + 2)
                {
                    heightFlag = 0;
                }
                else if (rightRoom.Height >= TextBoxMinHeight + 2)
                {
                    heightFlag = 1;
                }
                else
                {
                    heightFlag = 2;
                }

                if (widthFlag != 2 && heightFlag != 2)
                {
                    flag = 1;
                }
                else
                {
                    if (topRoom.Width >= fullWidth + 2)
                    {
                        widthFlag = 0;
                    }
                    else if (topRoom.Width >= TextBoxMinWidth + 2)
                    {
                        widthFlag = 1;
                    }
                    else
                    {
                        widthFlag = 2;
                    }

                    if (topRoom.Height >= fullHeight + 4)
                    {
                        heightFlag = 0;
                    }
                    else if (topRoom.Height >= TextBoxMinHeight + 4)
                    {
                        heightFlag = 1;
                    }
                    else
                    {
                        heightFlag = 2;
                    }

                    if (widthFlag != 2 && heightFlag != 2)
                    {
                        flag = 2;
                    }
                    else
                    {
                        if (leftRoom.Width >= fullWidth + 4)
                        {
                            widthFlag = 0;
                        }
                        else if (leftRoom.Width >= TextBoxMinWidth + 4)
                        {
                            widthFlag = 1;
                        }
                        else
                        {
                            widthFlag = 2;
                        }

                        if (leftRoom.Height >= fullHeight + 2)
                        {
                            heightFlag = 0;
                        }
                        else if (leftRoom.Height >= TextBoxMinHeight + 2)
                        {
                            heightFlag = 1;
                        }
                        else
                        {
                            heightFlag = 2;
                        }

                        if (widthFlag != 2 && heightFlag != 2)
                        {
                            flag = 3;
                        }
                        else
                        {
                            flag = 4;
                        }
                    }
                }
            }

            switch (flag)
            {
                case 0:
                    // 显然这里的 widthFlag 不可能等于 2
                    // flag 0~3 同理，heightFlag 同理
                    width = widthFlag == 0 ? fullWidth : bottomRoom.Width - 2;
                    height = heightFlag == 0 ? fullHeight : bottomRoom.Height - 4;
                    left = SelectRectangle.Right - width - 2;
                    top = SelectRectangle.Bottom;
                    break;
                case 1:
                    width = widthFlag == 0 ? fullWidth : rightRoom.Width - 4;
                    height = heightFlag == 0 ? fullHeight : rightRoom.Height - 2;
                    left = SelectRectangle.Right;
                    top = SelectRectangle.Top - 2;
                    break;
                case 2:
                    width = widthFlag == 0 ? fullWidth : topRoom.Width - 2;
                    height = heightFlag == 0 ? fullHeight : topRoom.Height - 4;
                    left = SelectRectangle.Right - width - 2;
                    top = SelectRectangle.Top - height - 4;
                    break;
                case 3:
                    width = widthFlag == 0 ? fullWidth : leftRoom.Width - 4;
                    height = heightFlag == 0 ? fullHeight : leftRoom.Height - 2;
                    left = SelectRectangle.Left - width - 4;
                    top = SelectRectangle.Top - 2;
                    break;
                case 4:
                    // 至于屏幕中央。
                    // 最大不超过一半，最小不超过四分之一，其余尺寸按实际尺寸算。
                    width = fullWidth < Width * 0.25 ? Width * 0.25 : (fullWidth > Width * 0.5 ? Width * 0.5 : fullWidth);
                    height = fullHeight < Height * 0.25 ? Height * 0.25 : (fullHeight > Height * 0.5 ? Height * 0.5 : fullHeight);
                    left = (Width - width) / 2;
                    top = (Height - height) / 2;
                    break;
            }

            TextResult.SetValue(Canvas.LeftProperty, left);
            TextResult.SetValue(Canvas.TopProperty, top);
            TextResult.Width = width;
            TextResult.Height = height;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region 截屏
        private Bitmap ScreenSnapshot;
        private void Snapshot()
        {
            var bitmap = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            ScreenSnapshot = bitmap;
            // ScreenSnapshot = new Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>(bitmap);
            Image.Source = Utils.Bitmap2ImageSource(bitmap);
        }
        #endregion

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseCommand.Execute(null);
            }
            else if (e.Key == Key.A)
            {
                UnselectCommand.Execute(null);
            }
        }
        //双击左键点击事件
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                MouseDClickCommand.Execute(null);
            }
        }

        
    }
}
