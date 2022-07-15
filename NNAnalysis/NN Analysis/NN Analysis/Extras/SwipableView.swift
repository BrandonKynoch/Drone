//
//  SwipableView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/14/22.
//

import SwiftUI

struct SwipableView: View {
    public let mainView: AnyView
    public let leftSide: AnyView?
    public let rightSide: AnyView?
    
    //Drag for suggestions
    private let horizontalDragMaxOffset: CGFloat = 130
    private let horizontalDragSensitivity: CGFloat = 0.9
    @State private var previousHorizontalDragOffset: CGFloat = 0
    @State private var horizontalDragOffset: CGFloat = 0
    
    var suggestionsDragGesture: some Gesture {
        DragGesture(minimumDistance: 30, coordinateSpace: .local).onChanged { val in
            if val.translation.width.magnitude > 0 {
                withAnimation {
                    horizontalDragOffset = (previousHorizontalDragOffset + (val.translation.width * horizontalDragSensitivity)).clamped(-horizontalDragMaxOffset...horizontalDragMaxOffset)
                }
            }
        }
        .onEnded { val in
            withAnimation {
                if abs(horizontalDragOffset) > horizontalDragMaxOffset * 0.8 {
                    horizontalDragOffset = horizontalDragMaxOffset * (val.translation.width.sign.rawValue > 0 ? -1 : 1)
                } else {
                    horizontalDragOffset = 0
                }
            }
            previousHorizontalDragOffset = horizontalDragOffset
        }
    }
    
    var body: some View {
        GeometryReader { geometry in
            ZStack {
                Group {
                    if horizontalDragOffset > 0 {
                        if leftSide != nil {
                            // left side
                            HStack {
                                leftSide!.frame(width: abs(horizontalDragOffset))
                                    .foregroundColor(.red)
                                    .onTapGesture {
                                        withAnimation {
                                            horizontalDragOffset = 0
                                        }
                                    }
                                
                                Spacer()
                            }
                        }
                    } else if horizontalDragOffset < 0 {
                        if rightSide != nil {
                            // right side
                            HStack {
                                Spacer()
                                
                                rightSide!.frame(width: abs(horizontalDragOffset))
                                    .foregroundColor(.green)
                                    .onTapGesture {
                                        withAnimation {
                                            horizontalDragOffset = 0
                                        }
                                    }
                            }
                        }
                    }
                }
                .frame(width: geometry.size.width)
                
                let showSideTab = ((horizontalDragOffset > 0 && leftSide != nil) || (horizontalDragOffset < 0 && rightSide != nil))
                
                mainView
                    .frame(width: showSideTab ? geometry.size.width - CGFloat(abs(horizontalDragOffset)) : geometry.size.width)
                    .offset(x: showSideTab ? horizontalDragOffset / 2 : 0)
                    .simultaneousGesture(suggestionsDragGesture)
            }
            .onAppear() {
                previousHorizontalDragOffset = 0
                horizontalDragOffset = 0
            }
        }
    }
}
//
//struct SwipableView_Previews: PreviewProvider {
//    static var previews: some View {
//        SwipableView()
//    }
//}
