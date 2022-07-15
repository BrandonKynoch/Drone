//
//  NNGraph.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/14.
//

import SwiftUI

struct NNGraph: View {
    @ObservedObject var nnGroup: NNGroup
    @State var i: Int = 0
    
    var body: some View {
        if let currentViewingNN = nnGroup.currentViewingNN {
            ZStack {
                BackgroundView(viewMode: .dark, opacity: 0.5)
                VStack {
                    // Neural shape
                    ZStack {
                        PanelSolidView(colour: S_COL_BACKGROUND2)
                        
                        HStack {
                            ForEach (currentViewingNN.neuralShape, id: \.self) { shape in
                                Spacer()
                                Text("\(shape)")
                                    .modifier(BodyTextModifier())
                                    .foregroundColor(S_COL_TEXT)
                                Spacer()
                            }
                        }
                        .padding()
                    }
                    .frame(height: 50)
                    .padding()
                    
                    BiasView(nn: currentViewingNN)
                        .padding()
                }
            }.cornerRadius(S_CORNER_RADIUS)
        }
    }
}

struct BiasView: View {
    let nn: NN
    
    var body: some View {
        GeometryReader { geometry in
            let frameHeight = geometry.size.height
            let frameWidth = geometry.size.width
            
            let layerSpacing = frameWidth / CGFloat(nn.neuralSize + 1)
            let xAxisScale = 5 * (frameWidth / 500)

            ForEach(nn.UIIndexArray, id: \.self) { i in
                let baseX = layerSpacing * CGFloat(i + 2)
                let ySegmentSpacing = frameHeight / CGFloat(nn.maxShape)
                let baseY = (frameHeight / 2) - ((CGFloat(nn.layers[i].biases.count) / 2) * ySegmentSpacing)
                Path { path in
                    path.move(to: CGPoint(
                        x: baseX,
                        y: baseY
                    ))
    
                    for biasI in 1..<nn.layers[i].biases.count {
                        path.addLine(to: CGPoint(
                            x: baseX + (nn.layers[i].biases[biasI] * xAxisScale),
                            y: baseY + CGFloat(biasI) * ySegmentSpacing
                        ))
                    }
                }
                .stroke(
                    S_COL_ACC2,
                    style: StrokeStyle(lineWidth: 3, lineCap: .round, lineJoin: .round)
                )
            }
        }
    }
}
