//
//  SwiftUIExtras.swift
//  Curator
//
//  Created by Brandon Kynoch on 2021/03/15.
//

import SwiftUI
import MapKit
//import MapItemPicker // https://github.com/lorenzofiamingo/swiftui-map-item-picker

// MARK: Global Modifiers
struct TextModifier: ViewModifier {
    let size: CGFloat
    let weight: Font.Weight
    
    func body(content: Content) -> some View {
        content
//            .font(.system(size: size, weight: weight, design: .default))
            .font(Font.custom("PT Mono", size: size).weight(weight))
            .modifier(ShadowModifier())
    }
}

struct TextModifierNoShadow: ViewModifier {
    let size: CGFloat
    let weight: Font.Weight
    
    func body(content: Content) -> some View {
        content
            .font(.system(size: size, weight: weight, design: .default))
    }
}

struct ItalicTextModifier: ViewModifier {
    let size: CGFloat
    let weight: Font.Weight
    
    func body(content: Content) -> some View {
        content
            .font(.system(size: size, weight: weight, design: .default).italic())
            .modifier(ShadowModifier())
    }
}

struct TitleTextModifier: ViewModifier {
    func body(content: Content) -> some View {
        content
            .modifier(TextModifier(size: 24, weight: .bold))
    }
}

struct BodyTextModifier: ViewModifier {
    func body(content: Content) -> some View {
        content
            .modifier(TextModifier(size: 16, weight: .medium))
    }
}

struct LightTextModifier: ViewModifier {
    func body(content: Content) -> some View {
        content
            .modifier(TextModifier(size: 16, weight: .ultraLight))
    }
}

struct ShadowModifier: ViewModifier {
    func body(content: Content) -> some View {
        content
            .shadow(color: SHADOW_COL, radius: 8, x: 0, y: -3)
    }
}



struct PanelView: View {
    var type: NSVisualEffectView.BlendingMode
    var body: some View {
        Rectangle()
            .backgroundGaussianBlur(type: type)
            .foregroundColor(Color.clear)
            .cornerRadius(CORNER_RADIUS)
            .modifier(ShadowModifier())
    }
}

@available(OSX 11.0, *)
public extension View {
    func backgroundGaussianBlur(type: NSVisualEffectView.BlendingMode = .withinWindow) -> some View {
        self
            .background( VisualEffectView(type: type) )
    }
}

@available(OSX 10.15, *)
public struct VisualEffectView: NSViewRepresentable {
    let type: NSVisualEffectView.BlendingMode
    
    public init(type: NSVisualEffectView.BlendingMode = .withinWindow) {
        self.type = type
    }
    
    public func makeNSView(context: Context) -> NSVisualEffectView {
        NSVisualEffectView()
    }
    
    public func updateNSView(_ nsView: NSVisualEffectView, context: Context) {
        nsView.blendingMode = type
        nsView.material = .popover
    }
    
    public typealias NSViewType = NSVisualEffectView
}





struct PanelSolidView: View {
    public let colour: Color
    
    var body: some View {
        Rectangle()
            .foregroundColor(colour)
            .cornerRadius(CORNER_RADIUS)
            .modifier(ShadowModifier())
    }
}

struct EditTextView<T>: View {
    let description: String
    @Binding var content: T?
    @State private var originalContent: T?
    @State private var isEditing: Bool = false
    private let customEditBeginNotification: String?
    private let customEditEndNotification: String?
    
    @ObservedObject private var validationField: ValidationFields.Entry
    
    private let style: Style
    
    // For ContentOnly mode
    let fontSize: CGFloat
    let fontWeight: Font.Weight
    
    private var onBeginEditNotification = NotificationCenter.default.publisher(for: .OnBeginEdit)
    private var onEndEditNotification = NotificationCenter.default.publisher(for: .OnEndEdit)
    
    public enum Style {
        case DescriptionContent
        case ContentOnly
        case Currency
    }
    
    init(description: String, content: Binding<T?>) {
        self.description = description
        self._content = content
        self.style = .DescriptionContent

        self.customEditBeginNotification = nil
        self.customEditEndNotification = nil
        
        self.fontSize = 16
        self.fontWeight = .bold
        
        self.validationField = ValidationFields.Entry.nilValidationField
        
        originalContent = content.wrappedValue
    }
    
    init(description: String, content: Binding<T?>, validation: ValidationFields.Entry) {
        self.description = description
        self._content = content
        self.style = .DescriptionContent

        self.customEditBeginNotification = nil
        self.customEditEndNotification = nil
        
        self.fontSize = 16
        self.fontWeight = .bold
        
        self.validationField = validation
        
        originalContent = content.wrappedValue
    }
    
    init(content: Binding<T?>, fontSize: CGFloat, fontWeight: Font.Weight, customEditBeginNotification: String, customEditEndNotification: String) {
        self._content = content
        self.fontSize = fontSize
        self.fontWeight = fontWeight
        self.style = .ContentOnly
        
        self.customEditBeginNotification = customEditBeginNotification
        self.customEditEndNotification = customEditEndNotification
        self.description = ""

        self.onBeginEditNotification = NotificationCenter.default.publisher(for: .init(rawValue: customEditBeginNotification))
        self.onEndEditNotification = NotificationCenter.default.publisher(for: .init(rawValue: customEditEndNotification))
        
        self.validationField = ValidationFields.Entry.nilValidationField
        
        originalContent = content.wrappedValue
    }
    
    init(description: String, content: Binding<T?>, style: Style) {
        self.description = description
        self._content = content
        self.style = style
        
        self.customEditBeginNotification = nil
        self.customEditEndNotification = nil
        
        self.fontSize = 16
        self.fontWeight = .bold
        
        self.validationField = ValidationFields.Entry.nilValidationField

        originalContent = content.wrappedValue
    }
    
    init(content: Binding<T?>) {
        self.description = "$"
        self._content = content
        self.style = .Currency
        
        self.customEditBeginNotification = nil
        self.customEditEndNotification = nil
        
        self.fontSize = 16
        self.fontWeight = .bold
        
        self.validationField = ValidationFields.Entry.nilValidationField
        
        originalContent = content.wrappedValue
    }
    
    var body: some View {
        VStack {
            if (content != nil || isEditing) && !(content == nil && !isEditing) {
                switch style {
                case .DescriptionContent:
                    HStack {
                        Text(description)
                            .modifier(TextModifier(size: 16, weight: .light))
                            .foregroundColor(validationField.valid ? .black : COL_CANCEL)
                        Spacer()
                    }
                    
                    HStack {
                        if !isEditing {
                            Text(content! as? String ?? "")
                                .modifier(TextModifier(size: 16, weight: .bold))
                        } else {
                            TextField(description, text: .init(
                                get: { return (content as? String) ?? "" },
                                set: { val in self.content = val as? T }))
                                .foregroundColor(validationField.valid ? COL_ACC2 : COL_CANCEL)
                                .background(validationField.valid ? .clear : COL_CANCEL.opacity(0.3))
                                .modifier(TextModifier(size: 16, weight: .bold))
                                .mask(Rectangle().cornerRadius(4))
                                .offset(y: -11)
                        }
                        Spacer()
                    }
                    
                    Spacer().frame(height: 10)
                case .ContentOnly:
                    HStack {
                        if !isEditing {
                            Text(content! as? String ?? "")
                                .foregroundColor(COL_ACC0)
                                .modifier(TextModifier(size: fontSize, weight: fontWeight))
                            Spacer()
                        } else {
                            TextEditor(text: .init(
                                get: { return (content as? String) ?? "" },
                                set: { val in self.content = val as? T }))
                                .foregroundColor(validationField.valid ? COL_ACC2 : COL_CANCEL)
                                .background(.clear)
                                .modifier(TextModifierNoShadow(size: fontSize, weight: fontWeight))
                        }
                    }
                case .Currency:
                    HStack {
                        Text(description)
                            .modifier(TextModifier(size: 27, weight: .light))
                        
                        Spacer().frame(width: 3)
                        
                        if !isEditing {
                            Text("\(content! as! Int)" as? String ?? "")
                                .modifier(TextModifier(size: 27, weight: .bold))
                        } else {
                            TextField("123", text: .init(
                                get: { return "\(content! as! Int)" as? String ?? "" },
                                set: { val in
                                    if let int = Int(val) {
                                        self.content = int as? T
                                    }
                                }))
                                .foregroundColor(validationField.valid ? COL_ACC2 : COL_CANCEL)
                                .modifier(TextModifier(size: 27, weight: .bold))
                        }
                        Spacer()
                    }
                }
            }
        }
        .onReceive(onBeginEditNotification) { val in
            guard !isEditing else { return }
            
            withAnimation {
                isEditing = true
            }
            
            originalContent = content
            
            initContentVal()
        }
        .onReceive(onEndEditNotification) { save in
            withAnimation {
                isEditing = false
            }
            
            clearContentValIfZeroed()
            
            if !Bool(save.object.debugDescription) {
                content = originalContent
            }
        }
    }
    
    func initContentVal() {
        if content == nil {
            if T.self == String.self {
                content = "" as? T
            } else if T.self == Int.self {
                content = 0 as? T
            }
        }
    }
    
    func clearContentValIfZeroed() {
        if T.self == String.self {
            if content as? String == "" {
                content = nil
            }
        }
        if T.self == Int.self {
            
        }
    }
}

struct TextSuggestionView<T>: View {
    private var suggestions: Trie<T>?
    @Binding var editText: String?
    let onTap: ((key: String, value: T)) -> ()
    
    init(editText: Binding<String?>, suggestions: Trie<T>?, onTap: @escaping ((key: String, value: T)) -> ()) {
        self._editText = editText
        self.suggestions = suggestions
        self.onTap = onTap
    }
    
    var body: some View {
        if let editText = editText, let suggestions = suggestions {
            var currentSuggestions = suggestions.find(prefix: editText.lowercased())
            
            ScrollView {
                VStack (spacing: 10) {
                    ForEach (currentSuggestions.filter { (key, value) in
                        return key.count != editText.count
                    }, id: \.key) { suggestion in
                        HStack {
                            Text(suggestion.key.capitalized)
                                .modifier(TextModifier(size: 16, weight: .light))
                                .foregroundColor(COL_ACC2)
                            
                            Spacer()
                        }
                        .contentShape(Rectangle())
                        .onTapGesture {
                            onTap(suggestion)
                        }
                    }
                    
                }
            }
        }
    }
}

struct CustomPickerView<E: CaseIterable & Hashable>: View {
    let rowHeight: CGFloat
    @Binding var selected: E
    @Binding var canEdit: Bool
    @State var isEditing: Bool = false
    @State var isEditingNoAnim: Bool = false
    @State var isEditingDelayed: Bool = false
    @State var index: Int = 0
    
    init(rowHeight: CGFloat, selected: Binding<E>, canEdit: Binding<Bool>) {
        self.rowHeight = rowHeight
        self._selected = selected
        self._canEdit = canEdit
        self.isEditing = false
        self.isEditingNoAnim = false
        self.isEditingDelayed = false
    }
    
    var body: some View {
        ZStack {
            GeometryReader { geometry in
                let cr = CORNER_RADIUS * 1.5
                let pp: CGFloat = 2 // path padding
                // Background
                Path() { p in
                    p.move(to: CGPoint(
                        x: pp,
                        y: cr)
                    )
                    
                    p.addQuadCurve(
                        to: CGPoint(
                            x: cr,
                            y: pp),
                        control: CGPoint(
                            x: pp,
                            y: pp
                        ))
                    
                    p.addLine(to: CGPoint(
                        x: geometry.size.width - cr,
                        y: pp)
                    )
                    
                    p.addQuadCurve(
                        to: CGPoint(
                            x: geometry.size.width - pp,
                            y: cr),
                        control: CGPoint(
                            x: geometry.size.width - pp,
                            y: pp
                        ))
                    
                    p.addLine(to: CGPoint(
                        x: geometry.size.width - pp,
                        y: geometry.size.height - cr)
                    )
                    
                    p.addQuadCurve(
                        to: CGPoint(
                            x: geometry.size.width - cr,
                            y: geometry.size.height - pp),
                        control: CGPoint(
                            x: geometry.size.width - pp,
                            y: geometry.size.height - pp
                        ))
                    
                    p.addLine(to: CGPoint(
                        x: cr,
                        y: geometry.size.height - pp)
                    )
                    
                    p.addQuadCurve(
                        to: CGPoint(
                            x: pp,
                            y: geometry.size.height - cr),
                        control: CGPoint(
                            x: pp,
                            y: geometry.size.height - pp
                        ))
                    
                    p.addLine(to: CGPoint(
                        x: pp,
                        y: cr)
                    )
                }
                .stroke(COL_ACC2.opacity(isEditing ? 1 : 0), lineWidth: 3)
                .modifier(ShadowModifier())
                .position(x: geometry.size.width / 2, y: geometry.size.height / 2)
                
                // Selected Panel
                VStack {
                    Spacer()
                        .frame(height: isEditing ? rowHeight * CGFloat(E.allCases.firstIndex(of: selected) as! Int) : 0)
                    Rectangle()
                        .frame(height: rowHeight)
                        .cornerRadius(CORNER_RADIUS)
                        .foregroundColor(COL_ACC2)
                        .modifier(ShadowModifier())
                    if !isEditing {
                        Spacer()
                    }
                }
                
                VStack (spacing: 0) {
                    ForEach(Array(E.allCases), id: \.self) { s in
                        let showSPred = isEditing || (!isEditing && s == selected)
                        
                        HStack (spacing: 0) {
                            Spacer()
                            Text("\(s)" as String)
                                .foregroundColor((s == selected) ? .white : COL_ACC2)
                                .modifier(TextModifier(size: 15, weight: .bold))
                                .scaleEffect((s == selected) ? CGSize(width: 1.2, height: 1.2) : CGSize(width: 1, height: 1))
                            Spacer()
                        }
                        .frame(height: (showSPred) ? rowHeight : 0)
                        .opacity((showSPred) ? 1 : 0)
                        .contentShape(Rectangle())
                        .onTapGesture {
                            if isEditingNoAnim {
                                withAnimation {
                                    selected = s
                                }
                                
                                setEdit(val: false)
                            } else {
                                setEdit(val: true)
                            }
                        }
                    }
                }
            }
        }
        .frame(height: rowHeight * (isEditingDelayed ? CGFloat(E.allCases.count) : 1))
        .onChange(of: self.canEdit, perform: { newValue in
            if !newValue {
                setEdit(val: newValue)
            }
        })
    }
    
    func setEdit(val: Bool) {
        if !val {
            index += 1
            let i = index
            DispatchQueue.main.asyncAfter(deadline: DispatchTime.now() + 0.5, execute: {
                withAnimation() {
                    if self.index == i {
                        isEditing = false
                    }
                }
            })
            DispatchQueue.main.asyncAfter(deadline: DispatchTime.now() + 1, execute: {
                withAnimation() {
                    if self.index == i {
                        isEditingDelayed = false
                    }
                }
            })
            isEditingNoAnim = isEditing
        } else {
            if canEdit {
                index += 1
                let i2 = index
                withAnimation {
                    isEditing = true
                }
                DispatchQueue.main.asyncAfter(deadline: DispatchTime.now() + 0.5, execute: {
                    withAnimation() {
                        if self.index == i2 {
                            isEditing = true
                        }
                    }
                })
                withAnimation() {
                    if self.index == i2 {
                        isEditingDelayed = true
                    }
                }
                isEditingNoAnim = isEditing
            }
        }
    }
}




struct CustomStringPickerView: View {
    let rowHeight: CGFloat
    @Binding var selected: String
    let selectionOptions: [String]
    @Binding var canEdit: Bool
    @State var isEditing: Bool = false
    @State var isEditingNoAnim: Bool = false
    @State var isEditingDelayed: Bool = false
    @State var index: Int = 0
    
    init(rowHeight: CGFloat, selected: Binding<String>, selectionOptions: [String], canEdit: Binding<Bool>) {
        self.rowHeight = rowHeight
        self._selected = selected
        self.selectionOptions = selectionOptions
        self._canEdit = canEdit
        self.isEditing = false
        self.isEditingNoAnim = false
        self.isEditingDelayed = false
    }
    
    var body: some View {
        ZStack {
            GeometryReader { geometry in
                let cr = CORNER_RADIUS * 1.5
                let pp: CGFloat = 2 // path padding
                // Background
                Path() { p in
                    p.move(to: CGPoint(
                        x: pp,
                        y: cr)
                    )
                    
                    p.addQuadCurve(
                        to: CGPoint(
                            x: cr,
                            y: pp),
                        control: CGPoint(
                            x: pp,
                            y: pp
                        ))
                    
                    p.addLine(to: CGPoint(
                        x: geometry.size.width - cr,
                        y: pp)
                    )
                    
                    p.addQuadCurve(
                        to: CGPoint(
                            x: geometry.size.width - pp,
                            y: cr),
                        control: CGPoint(
                            x: geometry.size.width - pp,
                            y: pp
                        ))
                    
                    p.addLine(to: CGPoint(
                        x: geometry.size.width - pp,
                        y: geometry.size.height - cr)
                    )
                    
                    p.addQuadCurve(
                        to: CGPoint(
                            x: geometry.size.width - cr,
                            y: geometry.size.height - pp),
                        control: CGPoint(
                            x: geometry.size.width - pp,
                            y: geometry.size.height - pp
                        ))
                    
                    p.addLine(to: CGPoint(
                        x: cr,
                        y: geometry.size.height - pp)
                    )
                    
                    p.addQuadCurve(
                        to: CGPoint(
                            x: pp,
                            y: geometry.size.height - cr),
                        control: CGPoint(
                            x: pp,
                            y: geometry.size.height - pp
                        ))
                    
                    p.addLine(to: CGPoint(
                        x: pp,
                        y: cr)
                    )
                }
                .stroke(COL_ACC2.opacity(isEditing ? 1 : 0), lineWidth: 3)
                .modifier(ShadowModifier())
                .position(x: geometry.size.width / 2, y: geometry.size.height / 2)
                
                // Selected Panel
                VStack {
                    Spacer()
                        .frame(height: isEditing ? rowHeight * CGFloat(selectionOptions.firstIndex(of: selected) as! Int) : 0)
                    Rectangle()
                        .frame(height: rowHeight)
                        .cornerRadius(CORNER_RADIUS)
                        .foregroundColor(COL_ACC2)
                        .modifier(ShadowModifier())
                    if !isEditing {
                        Spacer()
                    }
                }
                
                VStack (spacing: 0) {
                    ForEach(selectionOptions, id: \.self) { s in
                        let showSPred = isEditing || (!isEditing && s == selected)
                        
                        HStack (spacing: 0) {
                            Spacer()
                            Text(s)
                                .foregroundColor((s == selected) ? .white : COL_ACC2)
                                .modifier(TextModifier(size: 15, weight: .bold))
                                .scaleEffect((s == selected) ? CGSize(width: 1.2, height: 1.2) : CGSize(width: 1, height: 1))
                            Spacer()
                        }
                        .frame(height: (showSPred) ? rowHeight : 0)
                        .opacity((showSPred) ? 1 : 0)
                        .contentShape(Rectangle())
                        .onTapGesture {
                            if isEditingNoAnim {
                                withAnimation {
                                    selected = s
                                }
                                
                                setEdit(val: false)
                            } else {
                                setEdit(val: true)
                            }
                        }
                    }
                }
            }
        }
        .frame(height: rowHeight * (isEditingDelayed ? CGFloat(selectionOptions.count) : 1))
        .onChange(of: self.canEdit, perform: { newValue in
            if !newValue {
                setEdit(val: newValue)
            }
        })
    }
    
    func setEdit(val: Bool) {
        if !val {
            index += 1
            let i = index
            DispatchQueue.main.asyncAfter(deadline: DispatchTime.now() + 0.5, execute: {
                withAnimation() {
                    if self.index == i {
                        isEditing = false
                    }
                }
            })
            DispatchQueue.main.asyncAfter(deadline: DispatchTime.now() + 1, execute: {
                withAnimation() {
                    if self.index == i {
                        isEditingDelayed = false
                    }
                }
            })
            isEditingNoAnim = isEditing
        } else {
            if canEdit {
                index += 1
                let i2 = index
                withAnimation {
                    isEditing = true
                }
                DispatchQueue.main.asyncAfter(deadline: DispatchTime.now() + 0.5, execute: {
                    withAnimation() {
                        if self.index == i2 {
                            isEditing = true
                        }
                    }
                })
                withAnimation() {
                    if self.index == i2 {
                        isEditingDelayed = true
                    }
                }
                isEditingNoAnim = isEditing
            }
        }
    }
}



struct CustomDividierView: View {
    let geometrySize: CGSize
    let percentScale: CGFloat
    let orientation: Axis.Set
    
    var body: some View {
        if orientation == .vertical {
            Spacer().frame(height: 20)
            Rectangle()
                .frame(width: geometrySize.width * percentScale, height: 2)
                .modifier(ShadowModifier())
            Spacer().frame(height: 20)
        } else {
            Spacer().frame(width: 20)
            Rectangle()
                .frame(width: 2, height: geometrySize.height * percentScale)
                .modifier(ShadowModifier())
            Spacer().frame(width: 20)
        }
    }
}

struct CustomProgressView: View {
    @Binding public var progress: Double
    
    var body: some View {
        GeometryReader { g in
            ZStack {
                PanelView(type: .withinWindow)
                
                HStack {
                    Rectangle()
                        .foregroundColor(.white)
                        .cornerRadius(15)
                        .shadow(color: Color.black.opacity(0.25), radius: 6)
                    
                    Spacer()
                        .frame(width: g.size.width * (1 - progress.clamped(0...1)))
                }
            }
        }
    }
}

struct CustomToggleView: View {
    public let description: String
    @Binding public var val: Bool
    
    var body: some View {
        HStack {
            Image(systemName: val ? "checkmark.square.fill" : "square")
                .contentShape(Rectangle())
                .onTapGesture {
                    withAnimation {
                        val = !val
                    }
                }
            
            Spacer().frame(width: ELEMENT_SPACING)
            
            Text(description)
            
            Spacer()
        }
        .modifier(BodyTextModifier())
        .foregroundColor(COL_TEXT)
    }
}

struct CustomSliderView: View {
    @State private var progress: Double
    @State private var scaledProgress: Double
    @State private var lastProgress: Double
    
    private let minVal: Double
    private let maxVal: Double
    
    private var onValueChange: (Double) -> ()
    
    private var dragGesture: some Gesture {
        DragGesture().onChanged({ val in
            if let width = NSScreen.main?.frame.width {
                progress = lastProgress + ((val.translation.width / width) * (maxVal - minVal))
                progress = progress.clamped(minVal...maxVal)
                scaledProgress = (progress - minVal) / (maxVal - minVal)
            }
        }).onEnded({ _ in
            lastProgress = progress
            onValueChange(progress)
        })
    }
    
    init(initialVal: Double, minVal: Double, maxVal: Double, onValueChange: @escaping (Double) -> ()) {
        self.progress = initialVal
        self.lastProgress = initialVal
        self.minVal = minVal
        self.maxVal = maxVal
        self.scaledProgress = (initialVal - minVal) / (maxVal - minVal)
        self.onValueChange = onValueChange
    }
    
    var body: some View {
        HStack {
            CustomProgressView(progress: $scaledProgress)
            
            Text("\(Int(round(progress * 100)))%")
                .modifier(BodyTextModifier())
                .foregroundColor(.white)
                .frame(width: 50)
        }
        .gesture(dragGesture)
    }
}

//struct customEditableMapView: View {
//    @ObservedObject var mapLocation: MapLocation
//    @Binding var isEditing: Bool
//    @State var geometrySize: CGSize
//
//    @State private var showingPicker = false
//
//    init(mapLocation: MapLocation, isEditing: Binding<Bool>, geometrySize: CGSize) {
//        self._isEditing = isEditing
//        self.geometrySize = geometrySize
//        self.mapLocation = mapLocation
//    }
//
//    var body: some View {
//        VStack{
//            if (mapLocation.isSet || mapLocation.address != nil) {
//                customMapView(geometrySize: geometrySize, mapLocation: mapLocation)
//            } else {
//                if isEditing {
//                    let add = mapLocation.address ?? "Address"
//                    Rectangle()
//                        .foregroundColor(S_COL_MAIN)
//                        .frame(height: 80)
//                        .cornerRadius(S_CORNER_RADIUS)
//                        .overlay {
//                            Text(add)
//                                .foregroundColor(.white)
//                                .modifier(TextModifier(size: 16, weight: .bold))
//                                .padding(.leading)
//                                .padding(.trailing)
//                        }
//                }
//            }
//        }
//        .onTapGesture {
//            if isEditing {
//                showingPicker = true
//            }
//        }
//        .mapItemPicker(isPresented: $showingPicker) { item in
//            guard let item = item else {
//                print("Map item is nil")
//                return
//            }
//
//            mapLocation.setAddress(mapItem: item)
//        }
//    }
//}

//struct customMapView: View {
//    public var geometrySize: CGSize
//    @ObservedObject public var mapLocation: MapLocation
//
//    init(geometrySize: CGSize, mapLocation: MapLocation) {
//        self.mapLocation = mapLocation
//        self.geometrySize = geometrySize
//    }
//
//    var body: some View {
//        if let address = mapLocation.address {
//            VStack {
//                if mapLocation.isSet {
//                    let pins = [mapLocation]
//
//                    Map(coordinateRegion: .init(get: { return mapLocation.viewingRegion }, set: { r in mapLocation.viewingRegion = r }), annotationItems: pins) {
//                        MapMarker(coordinate: $0.coordinate)
//                    }
//                    .frame(height: 200)
//                    .padding(-10)
//                    .onTapGesture {
//                        if let url = URL(string: "maps://?saddr=&daddr=\(mapLocation.coordinate.latitude),\(mapLocation.coordinate.longitude)") {
//                            UIApplication.shared.openURL(url)
//                        }
//                    }
//                }
//
//                Rectangle()
//                    .foregroundColor(S_COL_MAIN)
//                    .frame(height: 80)
//                    .overlay {
//                        Text(address)
//                            .foregroundColor(.white)
//                            .modifier(TextModifier(size: 16, weight: .bold))
//                            .padding(.leading)
//                            .padding(.trailing)
//                    }
//            }
//            .mask(Rectangle().cornerRadius(S_CORNER_RADIUS))
//            .modifier(ShadowModifier())
//            .onAppear {
//                mapLocation.resetViewingRegion()
//            }
//        }
//    }
//}


class SwiftUIExtrasSingleton: ObservableObject {
    private static var privateSingleton: SwiftUIExtrasSingleton?
    public static var singleton: SwiftUIExtrasSingleton = {
        if privateSingleton == nil {
            privateSingleton = SwiftUIExtrasSingleton()
        }
        return privateSingleton!
    }()
    
    init() {
        guard SwiftUIExtrasSingleton.privateSingleton == nil else { return }
        SwiftUIExtrasSingleton.privateSingleton = self
    }
}

//struct s_SwiftUIExtras_Previews: PreviewProvider {
//    static var previews: some View {
//        Group {
//            swiftUIExtras_EnvPreview()
//        }
//        .environmentObject(SwiftUIExtrasSingleton.singleton)
//        .previewLayout(.fixed(width: 400, height: 300))
//    }
//}
//
//struct swiftUIExtras_EnvPreview: View {
//    @EnvironmentObject var env: SwiftUIExtrasSingleton
//    
//    var body: some View {
//        VStack {
////            CustomPickerView(
////                rowHeight: 60,
////                selected: .init(get: { return env.opType }, set: { value in env.opType = value }),
////                canEdit: .constant(true))
//            
//            EditTextView(content: .init(get: { return env.price }, set: { val in env.price = val }))
//        }
//        .padding()
//    }
//}
