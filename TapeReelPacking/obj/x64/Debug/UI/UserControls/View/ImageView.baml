   M S B A M L   `   `   `  ����J  FTapeReelPacking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null[2clr-namespace:TapeReelPacking.UI.UserControls.View$TapeReelPacking.UI.UserControls.View  5      4    .TapeReelPacking.UI.UserControls.View.ImageView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      X TXceed.Wpf.Toolkit, Version=3.8.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f47xceedtk)http://schemas.xceed.com/wpf/xaml/toolkit  5      >local2clr-namespace:TapeReelPacking.UI.UserControls.View  5        � 	Resources  5       �� Source�� 5   
   $5 //TapeReelPacking;component/ResourceControl.xaml?�6   %    6X   .��5      � -   5         �Name$ grd_Image_View��5      .+�5   
   � $��Stretch=�6   $��0,0,0,30q�6,   $��Stretch=�5      ���#1a1a1a}�. .+�5      ]� -   5         ]�Name$ panelSliderGray��5      $��20��61   $	��Top=�6=    K� ZIndex$ 2��6V   $y�	Collapsed=�5      ��. ���	#FF1B1B27 J� Opacity$ 1Q�5   D   .+�5      �� -   5         ��Name$ 	sldBinary��5      $��2 0 0 0q�6,   $	��250��5      $��20��5      
������ $	��Top=�5      $
��Left=�5       �� TickFrequency$ 1Q�5       � Maximum$	 255Q�6+   	 � SmallChange$	 1Q�69   ���	#FFBD3F24$��1q�6a   
 �� IsSnapToTickEnabled
 .  � Value 5      ��  �� Path$
 Text� �� ElementName$ sldValueShow�� �� Mode$ TwoWay=�5!      �� 5"      -   5         ��Name$ sldValueShow��5"      $��30��60   
������ 
��� ��� �� TextAlignment$ Center=�6�   $��10(�6�   ��6T   �� $ Value�$ 	sldBinary��$ TwoWay=�6�   5#      � 5&      -   5        $ grd_Dock��5&      $	��Top=�6%   $
��Left=�6=   
�����w�.+�5'      �� -   5         ��Name$ image��5'      $��0 0 0 0q�6'   $��Uniform=�5(      $	��Top=�5)      $
��Left=�5*       �� BitmapScalingMode$ NearestNeighbor=�5+      $ 1��5.      6)   � 5/      -   5        $ GridCoordinate��5/      $	��Top=�52      $
��Left=�53      $ 4��54      ��50      �� $ image��$ ActualHeight�$ OneWay=� �� IsAsync . ��51      �� $ image��$ ActualWidth�$ OneWay=� . 54      �� 55      -   5         ��Name$ GridOverlay��55      $	��Top=�58      $
��Left=�59      $ 2��5:      ��56      �� $ image��$ ActualHeight�$ OneWay=� . ��57      �� $ image��$ ActualWidth�$ OneWay=� . 5;      �� 5=      -	   5        $ 	gridteach��5=      $	��Top=�6,   $
��Left=�5>      $ 5��60   ��5?      �� $ 	imageView��$ ActualWidth�$ OneWay=���5@      �� $ 	imageView��$ ActualHeight�$ OneWay=�.+�5A      �� -
   5         ��Name$ 
controlWin��5A   %   $y�	Collapsed=�69   $��80��5B      $ 5��6%   $��10��5C      $��80��5D      $��10��5E       �� Left$	 200��5F      
 �� Top$	 220��5G         DesignerItemTemplate#��[  5H      .��5I      � .+�5J      �� 
���\\��$��-3q�6;   $��3��5K   (    J� IsHitTestVisible .  5M      �� 5N      -   5        $��	rotaPoint��5O   <   
���\\��$��	-6 -6 0 0q�5N   ;   $	��Top=�6N   $
��Left=�6f   $��6��5O   (    .  5Q      5R      5S      �� 5T      -   5        $��GridTeachOverlay��5T      ��5U      �� $ 	imageView��$ ActualWidth�$ OneWay=���5V      �� $ 	imageView��$ ActualHeight�$ OneWay=�6b   5W      5Z      �� 5[      -   5        $ 
GridResult��5[      $	��Top=�5^      $
��Left=�5_      $ 3��5`      ��5\      �� $ image��$ ActualHeight�$ OneWay=� . ��5]      �� $ image��$ ActualWidth�$ OneWay=� . 5`      �� 5d      -   5        $��resultTeach��5d      $��	10,30,0,0q�6,    �� Orientation$ Vertical=�6?   $ 2��6V   .+�5e      �� -   5         ��Name$ tbl_Status1��5e      ���	#FF1A46FF$��35(�6H   $
��Bold"�6V   6j   �� 5f      -   5        $ tbl_Status2��5f      ���	#FFFF0621$��25(�6H   $
��Bold"�6V   6j   5g      5h      � 5k   
   $��20��6   ���#212020$	 0.9Q�5l   $   $��Bottom=�5m       � ColumnDefinitions 5n      �� 5o      $��3* �6#   60   �� 5p      $��1* �6#   60   .+�5r      �� $��0��6   $ 
Horizontal=�6)   $��Center=�6B   $
��Left=�6]   .+�5s      �� $ 
Horizontal=�6   $	��250��67   $��0,0,0,0q�6C   $��Center=�5t      .+�5u      �� $
��Pos:��6    $��0,0,8,0q�5v       $
��Left=�5w       $��Center=�5x        ( $styleTextBoxTaskBarImageView_Content#���  5y       5z      �� 5{      -   5        $ tbl_Pos��5{   !   $��[0,0] Robot [0,0]��5|      $
��Auto��5}      $��0,0,2,0q�5~      $
��Left=�5       �� TextAlignment$
 Left=�5�      $��Center=�5�       & "styleTextBoxTaskBarImageView_Title#���  5�      5�      5�      �� 5�      $ 
Horizontal=�6   $	��110��66   $��Center=�6C   .+�5�      �� $��Gray: ��6    $��0,0,8,0q�5�       $��Right=�5�       $��Center=�5�       #���  5�       5�      �� 5�      -   5        $ tbl_Value_gray��5�   !   $	��[0]��5�      $
��auto��5�      $��0,0,2,0q�5�      $
��Left=�5�      $
 Left=�5�      $��Center=�5�      #���  5�      5�      5�      5�      �� 5�      $��1��6   $ 
Horizontal=�6)   $��Center=�6B   $��Right=�6]   .+�5�      �� $
��Time��6   $��10,0,8,0q�5�      $��Right=�5�      $��Center=�5�      #���  5�      6_   �� 5�      -   5        $ tbl_InspectTime��5�      $��0��5�      $��30��5�      $��0,0,5,0q�5�      $
��Left=�5�      $ Right=�5�      $��Center=�5�      #���  5�      6\   �� 5�      $��ms��6   $��0,0,10,0q�5�      $��20��6$   $��Right=�5�      $��Center=�5�      #���  5�      5�      5�      5�      5�      