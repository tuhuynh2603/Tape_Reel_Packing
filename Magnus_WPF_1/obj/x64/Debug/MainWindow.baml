   M S B A M L   `   `   `  ????E  ADragablz, Version=0.0.3.234, Culture=neutral, PublicKeyToken=null5(clr-namespace:Dragablz;assembly=DragablzDragablz  5   	   G CMagnus_WPF_1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:Magnus_WPF_1Magnus_WPF_1 5	   	   U/clr-namespace:Magnus_WPF_1.UI.UserControls.View!Magnus_WPF_1.UI.UserControls.View 5
   	      Magnus_WPF_1.MainWindow   5   	   e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   =dragablz(clr-namespace:Dragablz;assembly=Dragablz     5   	   g cXceed.Wpf.AvalonDock.Themes.Aero, Version=3.8.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4[ WXceed.Wpf.AvalonDock, Version=3.8.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4i	 eXceed.Wpf.AvalonDock.Themes.VS2013, Version=2.9.0.1, Culture=neutral, PublicKeyToken=3e4669d2f30244f4i
 eXceed.Wpf.AvalonDock.Themes.VS2010, Version=3.8.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4h dXceed.Wpf.AvalonDock.Themes.Metro, Version=3.8.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4E
avalonDock,http://schemas.xceed.com/wpf/xaml/avalondock   	 
  5   	   Lmc;http://schemas.openxmlformats.org/markup-compatibility/2006   	 
  5   	   _ [MaterialDesignThemes.Wpf, Version=1.0.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   &localclr-namespace:Magnus_WPF_1  5	   	   :view/clr-namespace:Magnus_WPF_1.UI.UserControls.View  5
   	   -   5          ? WindowState$  	Maximized=?5   	    ? 	Resources 5         runSequenceImage ? RowDefinitions ?? TextAlignment  BgButton ?? EndPoint ? MappingMode ?? 
StartPoint ? Color ? Offset  sb_OpenDialogUC	 ?? TargetProperty
 ?? 
TargetName9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O? KeyTime O? Value ?? Path ?? Mode ?? UpdateSourceTrigger  sb_CloseDialogUC  FocusVisual ?? Property ?? Value J? SnapsToDevicePixels ?? StrokeDashArray  MouseOverButtonStyle  CheckedButtonStyle  Button.Disabled.Background ?? Color  Button.Disabled.Border 	 Button.Disabled.Foreground??%?  &	        &	     &	 ?    &	 ?    &	 ?    &	 ?    &	 V    &	     &	 >    &		 a    ? 5   
   $??3q?6(    5      ?? 5      $??3* ?6    6.   ?? 5      $??1* ?6    6.   .+?5      ?? $??0??6   $??\Resources\start.png??6"   6B   ?? 5      $??1??6   $	??Run??6%   
??? ???$ Center=?6D   $??20(?6[   6k   5      ?? 5   
   $ 0.5,1&?6/   $ RelativeToBoundingBox=?6@   $ 0.5,0&?6d   .??5      ? $ 	#FFE47F27??6   $ 0.439Q?6-   ? 5      $ 	#FFD06F67??6   $ 0.808Q?6-   5      ?? 5!   
   .??5"      U? $	 (FrameworkElement.Height)?6,   $
 grd_Dialog_Settings??6b   .\?5#        $ 0??6'   $ 0Q?63     5$      $ 0:0:0.4??6'    69   ?? $ DialogUCHeight?$ TwoWay=?$ PropertyChanged=?5%      5&      ?? 5(   
   .??5)      U? $	 (FrameworkElement.Height)?6,   $
 grd_Dialog_Settings??6b   .\?5*        $ 0??6'    63   ?? $ DialogUCHeight?$ TwoWay=?$ PropertyChanged=?  5+      $ 0:0:0.2??6'   $ 0Q?69   5,      5-      ?? 5.   
   .?5/      ??  ? ?? 50      ?? 51      .R?52      ?? $??2q?6$    . #??? ??6J   $??1??53   $   $	 1 2R?68   54      56      57      ?? 58   
   $ 0.5,1&?6;   $ RelativeToBoundingBox=?6L   $ 0.5,0&?6p   .??59      ? $ 	#FF03D4F2??6   $ 0.439Q?6-   ? 5:      $ 	#FF17717E??6   $ 0.808Q?6-   5;      ?? 5<   
   $ 0.5,1&?69   $ RelativeToBoundingBox=?6J   $ 0.5,0&?6n   .??5=      ? $ 	#FFFFBA40??6   $ 0.439Q?6-   ? 5>      $ 	#FFBFAD5E??6   $ 0.808Q?6-   5?      ?? 5@   
   $ 	#FF282323??6=   ?? 5A   
   $ 	#FF0073B9??69   ?? 5B   
   $ 	#FF9F9F9F??6=   5n      .??5p      ? 
???????.+?5u   
   ? -   5         ?Name$ controllTabPanel??5u      $	??130??6)   $	??Top=?66   .+?5w         Dragablz.TabablzControl  -   5          Name$ tab_controls??5w   &   ???	#FF322F2E$
??Auto=?6T   ??5x      ?? 5y      $ 0.5,0.03&?6*   $ RelativeToBoundingBox=?5z   2   $ 0.5,0&?5{   2   .??5|      ? $ 	#FF342F2E??6'   $ 0.997Q?69   ? 5}      $ 	#FF2662B1??6'   $ 0.919Q?69   5~      .@?5?      ?? -   5        $??	tabRibbon??5?      $??Magnus??6+   
????  ?.??5?      ?? -   5        $??tabMasterView??5?   )    ?? Orientation$ 
Horizontal=?5?   !   {?. .+?5?      ?? 
???????$??0,0,1,0q?6:   $??0,3,0,0q?6T   ._?5?      0  *System.Windows.Controls.Ribbon.RibbonGroup  -   5        $??inspection_group??5?   *   ??5?   "   ?? 5?   &    ?? 
TargetType"  6,   .?5?   *   ??  ? ??$ 12(?6E    ?? Triggers 5?   *   ?? 5?   .    ?? Binding 6:   ?? $ IsUseSmallIcon?$ PropertyChanged=?$ TwoWay=? ?? Value True6?   .a?5?   2   ??  ? ??$ 11(?6M   5?   /   5?   '   ??5?   "   ?? 5?   &   -   5        $??groupInspection??5?   B   $??
Inspection??60   $??Microsoft New Tai Lue)?5?   4   ???	#FFD6F9FF$??13(?5?   K   $
??Bold"?5?   4    J? Effect 5?   *   -  'System.Windows.Media.Effects.BlurEffect  5?   .      Radius$	 0.1Q?69   5?   '   .@?5?   "   1  +System.Windows.Controls.Ribbon.RibbonButton  -   5        $??inspect_btn??5?   /      Label$ Inspect??5?   9      LargeImageSource$ \Resources\run-custom.png??5?   9       MouseOverBackground#  [ 5?   9   
???????$
??auto??5?   9   6F     5?   "   -	   5        $??inspect_offline_btn??5?   /   $ Offline Inspection??5?   9   $ \Resources\run-custom.png??5?   9   #  [ 5?   9   
???????$
??auto??5?   9   6F   7  1System.Windows.Controls.Ribbon.RibbonToggleButton  5?   "   -
   5        $??inspect_cycle_btn??5?   5   !   Label$! Inspect Cycle??5?   9   "   LargeImageSource$"" \Resources\camera-custom.png??5?   9   
???????#   CheckedBackground## [ 5?   9   $   MouseOverBackground#$ [ 5?   9   $
??auto??5?   9     5?   "   -   5        $??load_teach_image_btn??5?   /   $ Load Teach Image??5?   9   $' !\Resources\file-upload-custom.png??5?   9   
???????#  [ 5?   9   $
??auto??5?   9   6F     5?   "   -   5        $??save_teach_image_btn??5?   /   $ Save Teach Image??5?   9   $( "\Resources\download-box-custom.png??5?   9   
???????#  [ 5?   9   $
??auto??5?   9   6F     5?   "   -   5        $??enable_saveimage_btn??5?   5   $! Enable Saving Image??5?   9   $" \Resources\save-image.png??5?   9   #$ [ 5?   9   
???????## [ 5?   9   $
??auto??5?   9   % d? 	IsChecked$
% TrueY?5?   9     5?   "   -   5        $??teach_parameters_btn??5?   5   $! Teach Parameter??5?   9   $%" \Resources\book-open-custom.png??5?   9   #$ [ 5?   9   
???????## [ 5?   9   $
??auto??5?   9   5?   9     5?   "   -   5        $??mapping_parameters_btn??5?   5   $! Mapping Setting??5?   9   $%" \Resources\book-open-custom.png??5?   9   #$ [ 5?   9   
???????## [ 5?   9   $
??auto??5?   9   5?   9     5?   "   -   5        &  Name$& 	btn_teach??5?   /   $ Teach??5?   3   $, &\Resources\human-male-board-custom.png??5?   3   
???????#  [ 5?   3   $??68??5?   3     5?   "   -   5        $& btn_next_teach??5?   /   $
 Next??5?   3   $( "\Resources/step-forward-custom.png??5?   3   
???????{?.  #  [ 5?   3   $??68??5?   3     5?   "   -   5        $& btn_abort_teach??5?   /   $ Abort??5?   3   $) #\Resources/close-octagon-custom.png??5?   3   
???????{?.  #  [ 5?   3   $??68??5?   3   5?      5?      5?      5?      5?      d? 5     -   5        ' d?Name$' btn_run_sequence??5     #??[  65   $
??Auto??5     $	??100??6   $??	0,30,10,5q?6+   $??Right=?6>   
?????? 
???/???$??1q?5  H   $% FalseY?5     5     5     ? 5
     -   5        $ grd_Dialog_Settings??5
     ( K? ZIndex$( 3??5     $
??Left=?5     $	??Top=?5     ??5     ?? $ DialogUCWidth?$ TwoWay=?$ PropertyChanged=?z?5     T? 5     -   5        tt_DialogSettings??5  %   ??5     ?? 5     $ Black??6"   ) ?? Opacity$	) 0.6Q?60    5     3  -System.Windows.Media.Effects.DropShadowEffect  5     *   
BlurRadius$* 2Q?6#   +   ShadowDepth$+ 1Q?62   ,   Color$, 	#FFAEAEAE??6B   .+?5     ? -   5        $ grd_PopupDialog??5     6S   5     ? 5     -   5        $ Layout??5     $??	0 130 0 0q?6#   $??Stretch=?5      
???   ?- ? ColumnDefinitions- 5"     ?? 5#     $??2.46* ?6'   67   ?? 5$     $??1* ?6'   64   .+?5'     ? $??0??6   .+?5(     )  #Xceed.Wpf.AvalonDock.DockingManager  -   5        .  Name$. dockManager??5(  0   /   AllowMixedOrientation/ . 0   GridSplitterWidth$0 1Q?5+  (   1   GridSplitterHeight$1 1Q?5,  (   2   Layout2 5-  (   ?? *
layoutHPSF+$ PropertyChanged=?$ TwoWay=?3   Theme3 5.     1	 	 +Xceed.Wpf.AvalonDock.Themes.Vs2013DarkTheme	  5/     4   DocumentHeaderTemplate4 51     ?? 52     .R?53  "   ?? $ 
Horizontal=?6-   .+?54  &   ?? $??CharacterEllipsis=?6G   ??60   ?? *Title+6G   ?? 55  &   $??0 0 0 0q?6K   $??10(?6\   $
??Bold"?6j   ??60   ?? *	ContentId+6j   56  #   57     59     5:     ? 5<     $??1??6   $??0 0 0 0q?6'    5=     ?? 5>     $
??2.1* ?6(   68   ?? 5?     $??1* ?6(   66   .+?5B     ? $??0??6   .+?5C     ?? $??10q?6   $??Mapping Result??6+   
???????$??20(?6[   .??5J     ?? -   5        5 ??Name$5 canvas_Mapping??5J  1   $??10q?6%   $
??Left=?6n   .+?5K     ?? -   5        6 ??Name$6 border_boundingbox_focus??5K  %   6I   5N     5P     5R     ? 5S     $??1??6   .+?5U     ?? $
??Auto??6!   7 ?? CornerRadius$7 8??5V     $??3q?6*   
?????? 8 J? Opacity$	8 0.8Q?5W  2   $??0.2,0,0.4,0.2q?5X     ._?5Y     ]? ??. .+?5Z  "   ]? ??. $	??Top=?6A   .+?5[  &   5
  /Magnus_WPF_1.UI.UserControls.View.StatisticView
  -   5        9 
 Name$9 statisticView??5[  9   $	??Top=?5\  )   6@   5]  #   5^     5_     5`     5a     5e     5f     5g     