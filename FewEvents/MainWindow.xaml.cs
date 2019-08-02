using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Navigation;

using GMap.NET;
using GMap.NET.WindowsPresentation;
using Newtonsoft.Json;
using GMap.NET.MapProviders;

using System.Collections.Generic;
using System.Linq;

using System.Reflection;

using System.IO;
using System.Text;
using System.Diagnostics;



namespace FewEvents
{
    public static class Resolver
    {
        private static volatile bool _loaded;

        public static void RegisterDependencyResolver()
        {
            if (!_loaded)
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnResolve;
                _loaded = true;
            }
        }

        private static Assembly OnResolve(object sender, ResolveEventArgs args)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = String.Format("{0}.{1}.dll",
                execAssembly.GetName().Name,
                new AssemblyName(args.Name).Name);

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                int read = 0, toRead = (int)stream.Length;
                byte[] data = new byte[toRead];

                do
                {
                    int n = stream.Read(data, read, data.Length - read);
                    toRead -= n;
                    read += n;
                } while (toRead > 0);

                return Assembly.Load(data);
            }
        }
    }

    public partial class MainWindow : Window
    {
        private DateTime DT;
        private BD data;
        private short R = 280,
            time = -1;
        private double w, hh, mw, mh, scale, x, y;
        private Thickness margin = new Thickness(),
            docs_margin = new Thickness();
        private bool research = true,
            reinfo = true;
        private float sizeEl = 2.0f;
        private string focused = "";
        public short curCB = 0;

        private ObjList Tags;
        private ObjList[] Categories = new ObjList[2];

        private GMapControl gmap;
        private Grid MapPanel;

        private void NextFrame(object sender, RoutedEventArgs e)
        {
            if (data.currantFrame < 3)
            {
                if (data.currantFrame == 1)
                {
                    if (!reinfo || data.city.Equals(""))
                    {
                        data.currantFrame++;
                    }
                }

                if (((Button)sender).Name.Equals("button_NO"))
                {
                    data.city = "";
                    data.id = 1;

                    using (StreamWriter outputFile = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FewEvents\\geo.json")) outputFile.WriteLine("{\"location\":{\"data\":{\"city\":\"\",}}}");
                }

                data.currantFrame++;

                ShowFrame();
            }
            else if (data.currantFrame == 3)
            {
                Frame3.Visibility = Visibility.Hidden;
                Frame3.Opacity = 0.0f;

                ShowFrame();

                data.currantFrame++;
            }
            else
            {
                if (research)
                {
                    if ((sender as Button).Name.Equals("prev"))
                    {
                        data.PrevPage();
                    }
                    else if ((sender as Button).Name.Equals("next"))
                    {
                        data.NextPage();
                    }
                    else if ((sender as Button).Name.Equals("POST"))
                    {
                        reinfo = true;
                        data.Restore();
                    }
                    ShowFrame();
                }
            }
        }
        private void ChangeSize(object sender, RoutedEventArgs e)
        {
            CS();
        }
        private void Focus(object sender, RoutedEventArgs e)
        {
            if (research) FocusElement((FrameworkElement)sender, 150);
        }
        private void Lost_focus(object sender, RoutedEventArgs e)
        {
            focused = "";
        }
        private void Click(object sender, RoutedEventArgs e)
        {
            if (research)
            {
                research = false;
                Show(curtain, 300, 0);
                ShowFullDescription(Int32.Parse((sender as FrameworkElement).Name.Substring(2, 1)) - 1, 350);
            }
        }
        private void Click_abort(object sender, RoutedEventArgs e)
        {
            if (research)
            {
                research = false;
                Fade(curtain, 300, 0);
                FadeFullDescription(350);
            }
        }
        private void SetAutoCity(object sender, RoutedEventArgs e)
        {
            if (research) AutoCity();
        }
        private void StopWrite(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "Search":
                    {
                        if (time == -1) _timer(3);
                        else time = 0;

                        if (Search.Text.Length > 0) Fade(h1, 100, 0);
                        else Show(h1, 200, 0);
                    }
                    break;
                case "TagBox":
                    {
                        if (TagBox.Text.Length > 0) Fade(h3, 100, 0);
                        else Show(h3, 200, 0);
                    }
                    break;
                case "CategoryBox":
                    {
                        //if (CategoryBox.Text.Length > 0) Fade(h2, 100, 0);
                        //else Show(h2, 200, 0);
                    }
                    break;
            }
        }
        private void SetParametr(object sender, RoutedEventArgs e)
        {
            if (research)
            {
                ClearCB();

                if (((sender as Button).Background as SolidColorBrush).Color == Colors.AliceBlue)
                {
                    Reload();

                    (sender as Button).Background = new SolidColorBrush(Color.FromRgb(34, 124, 185));
                    (sender as Button).Foreground = new SolidColorBrush(Colors.AliceBlue);

                    switch ((sender as Button).Name)
                    {
                        case "Places":
                            {
                                data.PorE = true;

                                Events.Background = new SolidColorBrush(Colors.AliceBlue);
                                Events.Foreground = new SolidColorBrush(Colors.RoyalBlue);
                            }
                            break;
                        case "Events":
                            {
                                data.PorE = false;

                                Places.Background = new SolidColorBrush(Colors.AliceBlue);
                                Places.Foreground = new SolidColorBrush(Colors.RoyalBlue);
                            }
                            break;
                    }

                    reinfo = true;
                    data.Restore();

                    data.tag = "";
                    data.category = "";


                    ShowFrame();
                }
            }
        }
        private void About(object sender, RoutedEventArgs e)
        {
            if (research == true)
            {
                research = false;
                time = -1;
                data.city = Search.Text;

                data.currantFrame = 1;

                FillTextBlock(counter, "", 100);
                FillTextBlock(Total, "", 100);
                ShowFrame();
                Reload();
            }
        }

        private void MapZoom_Click(object sender, RoutedEventArgs e)
        {
            if (gmap.MaxZoom >= gmap.Zoom)
            {
                gmap.Zoom++;
            }
        }
        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
        private void MapPoint_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (gmap.MaxZoom >= gmap.Zoom)
                {
                    gmap.Zoom++;
                }
            }
            else if (e.Delta < 0)
            {
                if (gmap.MinZoom <= gmap.Zoom)
                {
                    gmap.Zoom--;
                }
            }
        }
        private void MapZoom_max(object sender, RoutedEventArgs e)
        {
            gmap.Zoom = gmap.MaxZoom;
        }
        private void CenterPoint(object sender, RoutedEventArgs e)
        {
            if ((int)(sender as FrameworkElement).Tag == -1)
            {
                double S_lat = 0, S_lng = 0;
                
                foreach (var item in gmap.Markers)
                {
                    S_lat += item.Position.Lat;
                    S_lng += item.Position.Lng;
                }

                S_lat /= gmap.Markers.Count();
                S_lng /= gmap.Markers.Count();

                gmap.Zoom = 5;
                gmap.Position = new PointLatLng(S_lat, S_lng);
            }
            else
            {
                gmap.Position = new PointLatLng(gmap.Markers[(int)(sender as FrameworkElement).Tag].Position.Lat, gmap.Markers[(int)(sender as FrameworkElement).Tag].Position.Lng);
            }
        }
        private void MapZoom_plus(object sender, RoutedEventArgs e)
        {
            if (gmap.MaxZoom >= gmap.Zoom)
            {
                gmap.Zoom++;
            }
        }
        private void MapZoom_minus(object sender, RoutedEventArgs e)
        {
            if (gmap.MinZoom < gmap.Zoom)
            {
                gmap.Zoom--;
            }
        }
        

        private void InitMap()
        {
            MapPanel = new Grid();
            MapPanel.Height = 200;

            gmap = new GMapControl();

            gmap.MouseWheel += UIElement_OnMouseWheel;
            gmap.MouseDoubleClick += MapZoom_Click;

            gmap.MapProvider = GMapProviders.YandexMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;

            gmap.MinZoom = 2;
            gmap.MaxZoom = 16;
            gmap.MouseWheelZoomType = MouseWheelZoomType.ViewCenter;

            gmap.ShowCenter = false;

            gmap.CanDragMap = true;
            gmap.DragButton = MouseButton.Left;

            map.Children.Add(MapPanel);
            MapPanel.Children.Add(gmap);
            Grid.SetRow(MapPanel, 1);
            MapPanel.Margin = new Thickness(2, 10, 2, 2);

            
            Button bn_plus = new Button();
            bn_plus.Style = (Style)FindResource("MapBn");
            bn_plus.Content = "+";
            bn_plus.Click += MapZoom_plus;
            bn_plus.Margin =  new Thickness(10, 10, 10, 35);

            Button bn_minus = new Button();
            bn_minus.Style = (Style)FindResource("MapBn");
            bn_minus.Content = "-";
            bn_minus.Click += MapZoom_minus;
            bn_minus.Margin = new Thickness(10);

            Button bn_center = new Button();
            bn_center.Style = (Style)FindResource("MapBn");
            bn_center.Content = "o";
            bn_center.Tag = -1;
            bn_center.Click += CenterPoint;
            bn_center.Margin = new Thickness(10, 10, 10, 60);

            MapPanel.Children.Add(bn_plus);
            Grid.SetRow(bn_plus, 0);

            MapPanel.Children.Add(bn_minus);
            Grid.SetRow(bn_minus, 0);

            MapPanel.Children.Add(bn_center);
            Grid.SetRow(bn_center, 0);
        }
        private void MapPoint(double lat, double lng)
        {
            MapPanel.Height = 200;
            gmap.Position = new PointLatLng(lat, lng);
            gmap.Zoom = 6;

            GMapMarker marker = new GMapMarker(new PointLatLng(lat, lng));

            Image mark = new Image();

            mark.Style = (Style)FindResource("marker");
            mark.Tag = gmap.Markers.Count;
            mark.MouseWheel += MapPoint_OnMouseWheel;
            mark.MouseLeftButtonDown += MapZoom_max;
            mark.MouseLeftButtonDown += CenterPoint;

            marker.Shape = mark;
            marker.ZIndex = 10;

            gmap.Markers.Add(marker);
        }
        private void HideMap()
        {
            MapPanel.Height = 0;
        }
        private void ClearMap()
        {
            gmap.Markers.Clear();
        }

        async Task NullLen(short sleep)
        {
            await Task.Delay(sleep);

            data.len = 0;
        }
        async Task _timer(short sec)
        {
            while (++time < sec)
            {
                await Task.Delay(1000);
            }

            if (time < 1000)
            {
                if (load2.Visibility != Visibility.Visible)
                    Show(load2, 400, 0);

                Search.Text = Search.Text.Trim();
                data.city = Search.Text;


                data.GetLocale();

                if (time == sec)
                {
                    await data.locale_signal.WaitAsync();
                    FillTextBox(Search, data.city, 200);
                }

                Check_Error();

                if (research) Fade(load2, 400, 1500);

                time = -1;
            };
        }
        async Task ShowFullDescription(int id, short duration)
        {
            ScaleTransform myScaleTransform = new ScaleTransform();

            Show(backIMG, 200, 100);

            duration /= 50;
            short y = 50, count;
            double move;

            Dispatcher.Invoke(() => docs.Visibility = Visibility.Visible);

            hl1_i.Visibility = Visibility.Collapsed;
            hl2_i.Visibility = Visibility.Collapsed;
            hl3_i.Visibility = Visibility.Collapsed;
            hl4_i.Visibility = Visibility.Collapsed;
            hl5_i.Visibility = Visibility.Collapsed;

            ClearMap();

            r1.Text = data.record.places[id].name;
                
            r2.Text = Environment.NewLine + "---------------------------------------------" + Environment.NewLine 
                + data.record.places[id].category.name + Environment.NewLine;

            count = (short)data.record.places[id].tags.Length;
            if (count != 0)
            {
                r2.Text += "(";
                foreach (var unit in data.record.places[id].tags) r2.Text += unit.name + ((--count > 0) ? ", " : "");
                r2.Text += ")";
            };
            r2.Text += Environment.NewLine + Environment.NewLine + Regex.Replace(data.record.places[id].description, "&nbsp;|<[^>]+>", string.Empty) + Environment.NewLine;

            if (data.PorE)
            {
                if (data.record.places[id]?.workingSchedule?._0 != null &&
                    data.record.places[id]?.workingSchedule?._1 != null &&
                    data.record.places[id]?.workingSchedule?._2 != null &&
                    data.record.places[id]?.workingSchedule?._3 != null &&
                    data.record.places[id]?.workingSchedule?._4 != null &&
                    data.record.places[id]?.workingSchedule?._5 != null &&
                    data.record.places[id]?.workingSchedule?._6 != null)
                {
                    r2.Text += Environment.NewLine + "---------------------------------------------" + Environment.NewLine + "Расписание работы" + Environment.NewLine;

                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._0.from);
                    r2.Text += "Пн: " + DT.Hour + ":" + DT.Minute + " - ";
                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._0.to);
                    r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._1.from);
                    r2.Text += "Вт: " + DT.Hour + ":" + DT.Minute + " - ";
                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._1.to);
                    r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._2.from);
                    r2.Text += "Ср: " + DT.Hour + ":" + DT.Minute + " - ";
                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._2.to);
                    r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._3.from);
                    r2.Text += "Ч: " + DT.Hour + ":" + DT.Minute + " - ";
                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._3.to);
                    r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._4.from);
                    r2.Text += "Пт: " + DT.Hour + ":" + DT.Minute + " - ";
                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._4.to);
                    r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._5.from);
                    r2.Text += "Сб: " + DT.Hour + ":" + DT.Minute + " - ";
                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._5.to);
                    r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._6.from);
                    r2.Text += "Вс: " + DT.Hour + ":" + DT.Minute + " - ";
                    DT = DateTime.Today + new TimeSpan(0, 0, data.record.places[id].workingSchedule._6.to);
                    r2.Text += DT.Hour + ":" + DT.Minute;
                }

                if (data.record.places[id].workingScheduleComment != null) r2.Text += Environment.NewLine + data.record.places[id].workingScheduleComment;

                r2.Text += Environment.NewLine + "---------------------------------------------" + Environment.NewLine;

                if (data.record.places[id].address != null)
                {
                    r2.Text += "Адрес : " + data.record.places[id].address.street + Environment.NewLine;

                    if (data.record.places[id].address.comment != null)
                        if (data.record.places[id].address.comment.Length > 0)
                            r2.Text += "(" + data.record.places[id].address.comment + ")" + Environment.NewLine;

                    if (data.record.places[id].address?.mapPosition?.coordinates != null)
                    {
                        MapPoint(data.record.places[id].address.mapPosition.coordinates[0], data.record.places[id].address.mapPosition.coordinates[1]);
                    }
                }

                if (data.record.places[id].contacts.phones != null) {
                    r2.Text += Environment.NewLine + "телефон :" + Environment.NewLine;

                    foreach (var unit in data.record.places[id].contacts.phones)
                    {
                        r2.Text += unit?.value + "  " + unit?.comment + Environment.NewLine;
                    }
                }

                if (data.record.places[id].contacts?.email != null)
                {
                    r2.Text += Environment.NewLine + "email : " + data.record.places[id].contacts.email + Environment.NewLine;
                }

                if (data.record.places[id].contacts?.website != null) {
                    hl1_i.Visibility = Visibility.Visible;
                    hl1.NavigateUri = new Uri(data.record.places[id].contacts.website);
                }

                if (data.record.places[id].contacts?.facebook != null)
                {
                    hl2_i.Visibility = Visibility.Visible;
                    hl2.NavigateUri = new Uri(data.record.places[id].contacts.facebook);
                }

                if (data.record.places[id].contacts?.twitter != null)
                {
                    hl3_i.Visibility = Visibility.Visible;
                    hl3.NavigateUri = new Uri(data.record.places[id].contacts.twitter);
                }

                if (data.record.places[id].contacts?.instagram != null)
                {
                    hl4_i.Visibility = Visibility.Visible;
                    hl4.NavigateUri = new Uri(data.record.places[id].contacts.instagram);
                }

                if (data.record.places[id].contacts?.vkontakte != null)
                {
                    hl5_i.Visibility = Visibility.Visible;
                    hl5.NavigateUri = new Uri(data.record.places[id].contacts.vkontakte);
                }
            }
            else
            {
                if (data.record.places[id].places != null)
                    r2.Text += Environment.NewLine + "---------------------МЕСТА-------------------" + Environment.NewLine;

                short num = 0;
                foreach (var place in data.record.places[id].places)
                {
                    r2.Text += ++num + ")  " + place.address.street + ((place.address.comment.Length > 0) ? (" (" + place.address.comment + ")") : "")
                                    + Environment.NewLine + Environment.NewLine;

                    if (place?.seances != null)
                        foreach (var seance in place.seances)
                            r2.Text += seance.startLocal + " - " + seance.endLocal + Environment.NewLine + Environment.NewLine;

                    if (place?.schedule != null)
                    {
                        if (place?.schedule?.days != null)
                            if (place.schedule.days?._0 != null &&
                                place.schedule.days?._1 != null &&
                                place.schedule.days?._2 != null &&
                                place.schedule.days?._3 != null &&
                                place.schedule.days?._4 != null &&
                                place.schedule.days?._5 != null &&
                                place.schedule.days?._6 != null)
                            {
                                r2.Text += Environment.NewLine + "---------------------------------------------" + Environment.NewLine + "Расписание работы" + Environment.NewLine;

                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._0.from);
                                r2.Text += "Пн: " + DT.Hour + ":" + DT.Minute + " - ";
                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._0.to);
                                r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._1.from);
                                r2.Text += "Вт: " + DT.Hour + ":" + DT.Minute + " - ";
                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._1.to);
                                r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._2.from);
                                r2.Text += "Ср: " + DT.Hour + ":" + DT.Minute + " - ";
                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._2.to);
                                r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._3.from);
                                r2.Text += "Ч: " + DT.Hour + ":" + DT.Minute + " - ";
                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._3.to);
                                r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._4.from);
                                r2.Text += "Пт: " + DT.Hour + ":" + DT.Minute + " - ";
                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._4.to);
                                r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._5.from);
                                r2.Text += "Сб: " + DT.Hour + ":" + DT.Minute + " - ";
                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._5.to);
                                r2.Text += DT.Hour + ":" + DT.Minute + Environment.NewLine;

                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._6.from);
                                r2.Text += "Вс: " + DT.Hour + ":" + DT.Minute + " - ";
                                DT = DateTime.Today + new TimeSpan(0, 0, place.schedule.days._6.to);
                                r2.Text += DT.Hour + ":" + DT.Minute;
                            }

                        r2.Text += Environment.NewLine + place.schedule.startLocal + " - " + place.schedule.endLocal + Environment.NewLine + Environment.NewLine;
                    }


                    if (place?.address?.mapPosition?.coordinates != null)
                    {
                        MapPoint(place.address.mapPosition.coordinates[0], place.address.mapPosition.coordinates[1]);


                        Console.WriteLine("point = " + place.address.mapPosition.coordinates[0] + " : " + place.address.mapPosition.coordinates[1]);
                    }
                }

                if (num == 0) HideMap();

                if (data.record.places[id]?.seances != null)
                {
                    r2.Text += Environment.NewLine + "-------------------СЕАНСЫ--------------------" + Environment.NewLine;

                    foreach (var seance in data.record.places[id].seances)
                    {
                        r2.Text += FromUnixTime(seance.start) + " - " + FromUnixTime(seance.end) + Environment.NewLine + Environment.NewLine;
                    }
                }

                r2.Text += "---------------------------------------------" + Environment.NewLine;

                if (data.record.places[id]?.isFree == true)
                {
                    r2.Text += "БЕСПЛАТНО";
                }
                else if (data.record.places[id]?.isFree == false)
                {
                    if (data.record.places[id]?.price != null && data.record.places[id]?.maxPrice != null)
                        r2.Text += "ЦЕНА : " + data.record.places[id].price + " - " + data.record.places[id].maxPrice;
                    else if (data.record.places[id]?.price != null && data.record.places[id]?.maxPrice == null)
                        r2.Text += "ЦЕНА : " + data.record.places[id].price;
                    else if (data.record.places[id]?.price == null && data.record.places[id]?.maxPrice != null)
                        r2.Text += "ЦЕНА : " + data.record.places[id].maxPrice;
                }


                if (data.record.places[id]?.organization?.name != null)
                    r2.Text += Environment.NewLine + "---------------------------------------------" +
                        Environment.NewLine + Environment.NewLine + "Организация : " + data.record.places[id].organization.name + Environment.NewLine;
            }


            while (y > 0)
            {
                await Task.Delay(duration);

                y -= 1;

                move = y*y*hh/2500;

                docs_margin.Bottom = move;
                docs_margin.Top = -move;
                docs.Margin = docs_margin;
            }
            

            research = true;
        }
        async Task FadeFullDescription(short duration)
        {
            ScaleTransform myScaleTransform = new ScaleTransform();

            Fade(backIMG, 200, 0);

            duration /= 50;
            short y = 0;
            double move;

            while (y < 50)
            {
                y += 1;

                await Task.Delay(duration);

                move = y * y * hh / 2500;

                docs_margin.Bottom = move;
                docs_margin.Top = -move;
                docs.Margin = docs_margin;
            }

            Dispatcher.Invoke(() => docs.Visibility = Visibility.Collapsed);

            GC.Collect();

            research = true;
        }
        async Task Show(FrameworkElement obj, short duration, short sleep)
        {
            if (obj.Visibility != Visibility.Visible)
            {
                await Task.Delay(sleep);

                float alpha = 0.0f;
                duration /= 20;

                Dispatcher.Invoke(() => obj.Visibility = Visibility.Visible);

                while (alpha < 1.0f)
                {
                    await Task.Delay(duration);
                    alpha += 0.05f;
                    Dispatcher.Invoke(() => obj.Opacity = alpha);
                }
            }
        }
        async Task ShowElement(FrameworkElement obj, FrameworkElement age_obj, TextBlock textB, short duration, short sleep, bool reverse, short ID)
        {
            ID--;

            y = Math.Cos(2 * 3.14 * ID / data.len);
            x = Math.Sqrt(1 - y * y) * R;

            y *= R;

            if (ID > data.len / 2)
            {
                margin.Left = 0;
                margin.Right = x;
            }
            else
            {
                margin.Right = 0;
                margin.Left = x;
            }

            if (y >= 0)
            {
                margin.Top = 0;
                margin.Bottom = y;
            }
            else
            {
                margin.Bottom = 0;
                margin.Top = -y;
            }

            (obj.Parent as Grid).Margin = margin;

            await Task.Delay(sleep);

            ScaleTransform myScaleTransform = new ScaleTransform();

            float size, z = 0.0f;
            duration /= 17;

            if (!reverse)
            {
                if (data.PorE) textB.Text = data.record.places[ID].category.name + Environment.NewLine + "(" + data.record.places[ID].status + ")";
                else
                {
                    if (data.record.places[ID]?.ageRestriction > 0)
                    {
                        (age_obj as TextBlock).Text = data.record.places[ID].ageRestriction.ToString() + "+";
                        Show(age_obj.Parent as FrameworkElement, 300, 700);
                    }
                    textB.Text = data.record.places[ID].category.name + Environment.NewLine;

                    if (data.record.places[ID]?.isFree == true)
                    {
                        textB.Text += "FREE";
                    }
                    else
                    {
                        if (data.record.places[ID]?.price != null && data.record.places[ID]?.maxPrice != null)
                            textB.Text += "ЦЕНА : " + data.record.places[ID].price + " - " + data.record.places[ID].maxPrice;
                        else if (data.record.places[ID]?.price != null && data.record.places[ID]?.maxPrice == null)
                            textB.Text += "ЦЕНА : " + data.record.places[ID].price;
                        else if (data.record.places[ID]?.price == null && data.record.places[ID]?.maxPrice != null)
                            textB.Text += "ЦЕНА : " + data.record.places[ID].maxPrice;
                    }
                }
            }
            else
            {
                if (data.record.places[ID]?.ageRestriction != 0)
                    Fade(age_obj.Parent as FrameworkElement, 300, 0);
            }

            while (z < 1.7)
            {
                await Task.Delay(duration);

                z += 0.1f;

                if (z < 1.4f) size = z * z;
                else size = (float)(Math.Sin(z * 25 + 3.14) * 0.05 + 1.95);

                if (reverse) size = 2 - size;

                myScaleTransform.ScaleX = size;
                myScaleTransform.ScaleY = size;

                myScaleTransform.CenterX = 50;
                myScaleTransform.CenterY = 50;

                obj.RenderTransform = myScaleTransform;
                Dispatcher.Invoke(() => obj.Opacity = size / 2);
            }

            if (reverse)
            {
                myScaleTransform.ScaleX = 0;
                myScaleTransform.ScaleY = 0;

                Dispatcher.Invoke(() => obj.Opacity = 0);
            }
            else
            {
                myScaleTransform.ScaleX = 2;
                myScaleTransform.ScaleY = 2;

                Dispatcher.Invoke(() => obj.Opacity = 1);
            }

            myScaleTransform.CenterX = 50;
            myScaleTransform.CenterY = 50;

            obj.RenderTransform = myScaleTransform;
        }
        async Task FocusElement(FrameworkElement obj, short duration)
        {
            ScaleTransform myScaleTransform = obj.RenderTransform as ScaleTransform;
            float z = 0;
            if (myScaleTransform.ScaleX > 2) z = (float)(Math.Tan((double)(3.14f * (myScaleTransform.ScaleX - 2.5f))) + 15) / 15.0f;
            short textZ = (short)(duration / 3);
            duration /= 20;

            short id = 0;
            TextBlock text_object = new TextBlock();
            DependencyObject age_obj = new DependencyObject();
            switch (obj.Name)
            {
                case "El1":
                    text_object = El1_text;
                    age_obj = El1_text1.Parent;
                    id = 0;
                    break;
                case "El2":
                    text_object = El2_text;
                    age_obj = El2_text1.Parent;
                    id = 1;
                    break;
                case "El3":
                    text_object = El3_text;
                    age_obj = El3_text1.Parent;
                    id = 2;
                    break;
                case "El4":
                    text_object = El4_text;
                    age_obj = El4_text1.Parent;
                    id = 3;
                    break;
                case "El5":
                    text_object = El5_text;
                    age_obj = El5_text1.Parent;
                    id = 4;
                    break;
                case "El6":
                    text_object = El6_text;
                    age_obj = El6_text1.Parent;
                    id = 5;
                    break;
                case "El7":
                    text_object = El7_text;
                    age_obj = El7_text1.Parent;
                    id = 6;
                    break;
                case "El8":
                    text_object = El8_text;
                    age_obj = El8_text1.Parent;
                    id = 7;
                    break;
            }

            Color c = (text_object.Foreground as SolidColorBrush).Color;

            focused = obj.Name;

            string str = "";
            if (data.PorE)
            {
                if (data.record.places[id].name.Length > 30) str = data.record.places[id].name.Substring(0, 30) + "...";
                else str = data.record.places[id].name;
            }
            else
            {
                if (data.record.places[id]?.ageRestriction > 0)
                    (age_obj as FrameworkElement).Opacity = 3 - myScaleTransform.ScaleX;

                if (data.record.places[id].name.Length > 30) str = data.record.places[id].name.Substring(0, 30) + "...";
                else str = data.record.places[id].name;
            }

            while (z < 2.0f && focused.Equals(obj.Name))
            {
                await Task.Delay(duration);

                z += 0.1f;

                sizeEl = (float)(Math.Atan((double)(z * 15 - 15)) / 3.14 + 2.5f);
                text_object.Foreground = new SolidColorBrush(Color.FromArgb((byte)(Math.Cos(z * 3.14) * 128 + 127), c.R, c.G, c.B));
                if (data.record.places[id]?.ageRestriction > 0)
                    (age_obj as FrameworkElement).Opacity = 1 - z * 0.5f;

                if (z > 0.8f && z < 1.0f)
                    if (data.PorE) text_object.Text = str;
                    else text_object.Text = str;

                myScaleTransform.ScaleX = sizeEl;
                myScaleTransform.ScaleY = sizeEl;

                myScaleTransform.CenterX = 50;
                myScaleTransform.CenterY = 50;

                obj.RenderTransform = myScaleTransform;
            }

            while (focused.Equals(obj.Name)) await Task.Delay(200);

            while (z > 0.0f && !focused.Equals(obj.Name))
            {
                await Task.Delay(duration);

                z -= 0.1f;

                sizeEl = (float)(Math.Atan((double)(z * 15 - 15)) / 3.14 + 2.5f);
                text_object.Foreground = new SolidColorBrush(Color.FromArgb((byte)(Math.Cos(z * 3.14) * 128 + 127), c.R, c.G, c.B));
                if (data.record.places[id]?.ageRestriction != 0)
                    (age_obj as FrameworkElement).Opacity = 1 - z * 0.5f;

                if (z > 0.7f && z < 0.9f)
                    if (data.PorE) text_object.Text = data.record.places[id].category.name + Environment.NewLine + data.record.places[id].status;
                    else text_object.Text = data.record.places[id].category.name + Environment.NewLine
                            + ((data.record.places[id].isFree) ? "FREE" : (data.record.places[id].price + " - " + data.record.places[id].maxPrice));

                myScaleTransform.ScaleX = sizeEl;
                myScaleTransform.ScaleY = sizeEl;

                myScaleTransform.CenterX = 50;
                myScaleTransform.CenterY = 50;

                obj.RenderTransform = myScaleTransform;
            }
        }
        async Task Fade(FrameworkElement obj, short duration, short sleep)
        {
            if (obj.Visibility != Visibility.Collapsed)
            {
                await Task.Delay(sleep);

                float alpha = 1.0f;
                duration /= 20;

                while (alpha > 0.0f)
                {
                    await Task.Delay(duration);
                    alpha -= 0.05f;
                    Dispatcher.Invoke(() => obj.Opacity = alpha);
                }

                Dispatcher.Invoke(() => obj.Visibility = Visibility.Collapsed);
            }
        }
        async Task FillTextBlock(TextBlock obj, string str, short duration)
        {
            short z = 0;
            duration /= 20;

            Color c = (obj.Foreground as SolidColorBrush).Color;


            while (z++ < 20)
            {
                await Task.Delay(duration);

                obj.Foreground = new SolidColorBrush(Color.FromArgb((byte)(Math.Cos(z * 0.314) * 128 + 128), c.R, c.G, c.B));

                if (z == 10) obj.Text = str;
            }
        }
        async Task FillTextBox(TextBox obj, string str, short duration)
        {
            short z = 0;
            duration /= 20;

            Color c = (obj.Foreground as SolidColorBrush).Color;


            while (z++ < 20)
            {
                await Task.Delay(duration);

                obj.Foreground = new SolidColorBrush(Color.FromArgb((byte)(Math.Cos(z * 0.314) * 128 + 128), c.R, c.G, c.B));

                if (z == 10)
                {
                    obj.Text = str;
                    obj.CaretIndex = str.Length;
                }
            }
        }
        async Task ProvideElements()
        {
            if (research)
            {
                research = false;

                LoadTag();
                LoadCategory();
                Show(load2, 400, 100);

                if (data.len >= 1) ShowElement(El1, El1_text1, El1_text, 200, 0, true, 1);
                if (data.len >= 2) ShowElement(El2, El2_text1, El2_text, 200, 75, true, 2);
                if (data.len >= 3) ShowElement(El3, El3_text1, El3_text, 200, 150, true, 3);
                if (data.len >= 4) ShowElement(El4, El4_text1, El4_text, 200, 225, true, 4);
                if (data.len >= 5) ShowElement(El5, El5_text1, El5_text, 200, 300, true, 5);
                if (data.len >= 6) ShowElement(El6, El6_text1, El6_text, 200, 375, true, 6);
                if (data.len >= 7) ShowElement(El7, El7_text1, El7_text, 200, 450, true, 7);
                if (data.len >= 8) ShowElement(El8, El8_text1, El8_text, 200, 525, true, 8);


                if (data.currantFrame == 3 || data.city != Search.Text.Trim())
                {
                    if (time == -1 || time == 100)
                    {
                        await _timer(0);
                    }
                    else if (time < 3)
                    {
                        time = 100;
                        await data.locale_signal.WaitAsync();
                        FillTextBox(Search, data.city, 200);
                        Check_Error();

                        time = -1;
                    }
                }

                await Task.Delay(500);
                data.GetRecord();
                await data.rec_signal.WaitAsync();
                Check_Error();
                await Task.Delay(700);

                if (data.len >= 1) ShowElement(El1, El1_text1, El1_text, 200, 0, false, 1);
                if (data.len >= 2) ShowElement(El2, El2_text1, El2_text, 200, 75, false, 2);
                if (data.len >= 3) ShowElement(El3, El3_text1, El3_text, 200, 150, false, 3);
                if (data.len >= 4) ShowElement(El4, El4_text1, El4_text, 200, 225, false, 4);
                if (data.len >= 5) ShowElement(El5, El5_text1, El5_text, 200, 300, false, 5);
                if (data.len >= 6) ShowElement(El6, El6_text1, El6_text, 200, 375, false, 6);
                if (data.len >= 7) ShowElement(El7, El7_text1, El7_text, 200, 450, false, 7);
                if (data.len >= 8) ShowElement(El8, El8_text1, El8_text, 200, 525, false, 8);

                if (prev.Visibility == Visibility.Collapsed)
                {
                    if (data.page > 0) Show(prev, 400, 0);
                }
                else if (data.page <= 1) Fade(prev, 400, 0);

                if (next.Visibility == Visibility.Collapsed)
                {
                    if (data.total > 8 * data.page && data.len == 8) Show(next, 400, 0);
                }
                else if (data.len < 8) Fade(next, 400, 0);

                FillTextBlock(Total, data.total.ToString(), 400);

                if (data.total != 0) FillTextBlock(counter, ((data.page * 8 + 1) + ".." + (data.page * 8 + data.len)).ToString(), 400);
                else FillTextBlock(counter, "", 400);

                Fade(load2, 300, 400);

                await Task.Delay(1000);

                research = true;
            }
        }
        async Task AutoCity()
        {
            research = false;
            Show(load2, 400, 0);

            data.GetCity();
            
            await data.city_signal.WaitAsync();

            Fade(load2, 400, 500);

            Check_Error();
            FillTextBox(Search, data.city, 300);

            research = true;
        }
        async Task ShowNext(short duration, short sleep)
        {
            await data.city_signal.WaitAsync();
            Check_Error();

            await Task.Delay(sleep);


            float alpha = 0.0f;
            duration /= 20;

            Dispatcher.Invoke(() => skip.Visibility = Visibility.Visible);

            while (alpha < 1.0f)
            {
                await Task.Delay(duration);
                alpha += 0.05f;
                Dispatcher.Invoke(() => skip.Opacity = alpha);
                Dispatcher.Invoke(() => load.Opacity = 1.0f - alpha);
            }

            Dispatcher.Invoke(() => load.Visibility = Visibility.Collapsed);
        }

        

        public MainWindow()
        {
            Resolver.RegisterDependencyResolver();
            GetCM();

            InitializeComponent();

            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            data = new BD();
            ShowFrame();
            InitMap();
        }

        private void KeyDownBlock(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                e.Handled = true;
            }
        }
        private void CS()
        {
            if (WindowState == WindowState.Maximized)
            {
                w = System.Windows.SystemParameters.PrimaryScreenWidth;
                hh = System.Windows.SystemParameters.PrimaryScreenHeight;
            }
            else
            {
                w = FE_Window.Width;
                hh = FE_Window.Height;
            }

            if (w > hh)
            {
                mh = hh / 10;
                mw = (w - hh) / 2 + mh;
                scale = hh / 480;
            }
            else
            {
                mh = (w - hh) / 2 + mw;
                mw = w / 10;
                scale = w / 500;
            }

            margin.Left = mw;
            margin.Right = mw;
            margin.Top = mh;
            margin.Bottom = mh;



            switch (data.currantFrame)
            {
                case 1:             //start - welcome
                    {
                        Frame1.Margin = margin;
                    }
                    break;
                case 2:             //verifing geolocating
                    {
                        Frame2.Margin = margin;
                    }
                    break;
                case 3:             //main frame of requests
                case 4:
                    {
                        ScaleFactor.CenterX = w / 2;
                        ScaleFactor.CenterY = hh / 2;

                        ScaleFactor.ScaleX = scale;
                        ScaleFactor.ScaleY = scale;

                        backIMG.Width = w / 11;
                        backIMG.Margin = new Thickness(w / 50);
                    }
                    break;
            }
        }

        private void RequestNavigator(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void ShowFrame()
        {
            switch (data.currantFrame)
            {
                case 1:             //start - welcome
                    {
                        if (Frame3.Visibility == Visibility.Visible)
                        {
                            if (data.len >= 1) ShowElement(El1, El1_text1, El1_text, 100, 0, true, 1);
                            if (data.len >= 2) ShowElement(El2, El2_text1, El2_text, 100, 25, true, 2);
                            if (data.len >= 3) ShowElement(El3, El3_text1, El3_text, 100, 50, true, 3);
                            if (data.len >= 4) ShowElement(El4, El4_text1, El4_text, 100, 75, true, 4);
                            if (data.len >= 5) ShowElement(El5, El5_text1, El5_text, 100, 100, true, 5);
                            if (data.len >= 6) ShowElement(El6, El6_text1, El6_text, 100, 125, true, 6);
                            if (data.len >= 7) ShowElement(El7, El7_text1, El7_text, 100, 150, true, 7);
                            if (data.len >= 8) ShowElement(El8, El8_text1, El8_text, 100, 175, true, 8);

                            NullLen(300);

                            reinfo = false;

                            Fade(Frame3, 500, 0);
                            Fade(About_bn, 300, 0);

                            if (skip.Visibility != Visibility.Visible)
                            {
                                Show(skip, 400, 0);
                            }
                        }
                        else
                        {
                            Show(load, 400, 1000);
                            ShowNext(400, 2000);
                        }

                        Show(Frame1, 500, 200);
                    }
                    break;
                case 2:             //verifing geolocating
                    {
                        Dispatcher.Invoke(() => geo_text.Text = "Ваш город - " + data.city);

                        Fade(Frame1, 500, 0);
                        Show(Frame2, 500, 200);
                    }
                    break;
                case 3:             //main frame of requests
                    {
                        research = true;

                        Show(About_bn, 300, 500);

                        Search.Text = data.city;

                        if (Search.Text.Length == 0) Show(h1, 200, 0);

                        CS();
                        Fade(Frame1, 500, 0);
                        Fade(Frame2, 500, 0);
                        Show(Frame3, 500, 200);

                        ProvideElements();

                        data.currantFrame++;
                    }
                    break;
                case 4:
                    {
                        GC.Collect();

                        ProvideElements();
                    }
                    break;
            }
        }
        private void Check_Error()
        {
            if (!data.error_message.Equals(""))
            {
                if (data.error_message.Equals("город не найден"))
                {
                    error_label.Content = "\"" + data.city + "\" не найден";
                    FillTextBox(Search, "", 200);
                    data.city = "";
                }
                else error_label.Content = "проверьте соединение сети";

                Show(error_label, 200, 0);
                Fade(error_label, 200, 3000);

                data.error_message = "";
                research = true;
            }
        }
        private DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }

        async Task GetCM()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream("FewEvents.tags.json"))
            using (StreamReader reader = new StreamReader(stream, Encoding.Default))
            {
                Tags = JsonConvert.DeserializeObject<ObjList>(reader.ReadToEnd());
            }

            using (Stream stream = assembly.GetManifestResourceStream("FewEvents.category_E.json"))
            using (StreamReader reader = new StreamReader(stream, Encoding.Default))
            {
                Categories[0] = JsonConvert.DeserializeObject<ObjList>(reader.ReadToEnd());
            }

            using (Stream stream = assembly.GetManifestResourceStream("FewEvents.category_P.json"))
            using (StreamReader reader = new StreamReader(stream, Encoding.Default))
            {
                Categories[1] = JsonConvert.DeserializeObject<ObjList>(reader.ReadToEnd());
            }
        }
        async Task LoadTag()
        {
            TagBox.ItemsSource = Tags.tags.ToList<Obj>();
        }
        async Task LoadCategory()
        {
            CategoryBox.ItemsSource = Categories[Convert.ToInt16(data.PorE)].tags.ToList<Obj>();
        }

        public async Task Reload()
        {
            TagBox.Text = "";
            data.tag = "";

            Show(h3, 200, 0);
            Fade(h333, 100, 0);

            foreach (var item in Tags.tags)
            {
                item.Check_Status = false;
            }

            LoadTag();

            CategoryBox.Text = "";
            data.category = "";

            Show(h2, 200, 0);
            Fade(h222, 100, 0);

            foreach (var item in Categories[Convert.ToInt16(data.PorE)].tags)
            {
                item.Check_Status = false;
            }
            LoadCategory();
        }
        public async Task ClearCB()
        {
            switch (curCB)
            {
                case 1:
                    {
                        TagBox.Text = "";
                        data.tag = "";

                        Show(h3, 200, 0);
                        Fade(h333, 100, 0);

                        foreach (var item in Tags.tags)
                        {
                            item.Check_Status = false;
                        }

                        LoadTag();
                    }
                    break;
                case 2:
                    {
                        CategoryBox.Text = "";
                        data.category = "";

                        Show(h2, 200, 0);
                        Fade(h222, 100, 0);

                        foreach (var item in Categories[Convert.ToInt16(data.PorE)].tags)
                        {
                            item.Check_Status = false;
                        }
                        LoadCategory();
                    }
                    break;
            }
        }
        public async Task ShowCB()
        {
            switch (curCB)
            {
                case 1:
                    {
                        TagBox.IsDropDownOpen = true;
                        Fade(h33, 100, 0);
                    }
                    break;
                case 2:
                    {
                        CategoryBox.IsDropDownOpen = true;
                        Fade(h22, 100, 0);
                    }
                    break;
            }
        }

        private void TagBox_GotMouseCapture(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).Name.Equals("TagBox")) curCB = 1;
            else if ((sender as FrameworkElement).Name.Equals("CategoryBox")) curCB = 2;
        }
        private void TagBox_LostMouseCapture(object sender, RoutedEventArgs e)
        {
            switch (curCB)
            {
                case 1:
                    {
                        Show(h33, 100, 0);
                    }
                    break;
                case 2:
                    {
                        Show(h22, 100, 0);
                    }
                    break;
            }

            curCB = 0;
        }
        private void ddl_TextChanged(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).Name.Equals("TagBox"))
            {
                TagBox.IsDropDownOpen = true;
                TagBox.ItemsSource = Tags.tags.Where(x => Regex.IsMatch(x.name.ToLower(), TagBox.Text.ToLower().Trim()));

                if (TagBox.Text.Length > 0) Fade(h3, 100, 0);
                else Show(h3, 200, 0);
            }
            else
            {
                CategoryBox.IsDropDownOpen = true;
                CategoryBox.ItemsSource = Categories[Convert.ToInt16(data.PorE)].tags.Where(x => Regex.IsMatch(Regex.Replace(x.name.ToLower(), "\\\\", ""), CategoryBox.Text.ToLower().Trim()));

                if (CategoryBox.Text.Length > 0) Fade(h2, 100, 0);
                else Show(h2, 200, 0);
            }
        }
        private void AllCheckbocx_CheckedAndUnchecked(object sender, RoutedEventArgs e)
        {
            List<string> ls = new List<string>();

            if ((sender as FrameworkElement).Name.Equals("CB_items"))
            {
                foreach (var item in Categories[Convert.ToInt16(data.PorE)].tags)
                {
                    if (item.Check_Status == true)
                    {
                        ls.Add(item.sysName);
                    }
                }

                if (ls.Count > 1) data.category = ls.Aggregate((x, y) => x + "," + y);
                else if (ls.Count == 1) data.category = ls[0];
                else data.category = "";


                if (ls.Count > 0)
                {
                    Show(h222, 100, 0);
                    (h222.Child as TextBlock).Text = ls.Count.ToString();
                }
                else
                {
                    Fade(h222, 100, 0);
                }
            }
            else
            {
                foreach (var item in Tags.tags)
                {
                    if (item.Check_Status == true)
                    {
                        ls.Add(item._id.ToString());
                    }
                }

                if (ls.Count > 1) data.tag = ls.Aggregate((x, y) => x + "," + y);
                else if (ls.Count == 1) data.tag = ls[0];
                else data.tag = "";

                if (ls.Count > 0)
                {
                    Show(h333, 100, 0);
                    (h333.Child as TextBlock).Text = ls.Count.ToString();
                }
                else
                {
                    Fade(h333, 100, 0);
                }
            }
        }
    }
}
