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
                            ForEach(currentViewingNN.UICountArray, id: \.self) { i in
                                Spacer()
                                Text("\(currentViewingNN.neuralShape[i])")
                                    .modifier(BodyTextModifier())
                                    .foregroundColor(S_COL_TEXT)
                                Spacer()
                            }
                        }
                        .padding()
                    }
                    .frame(height: 50)
                    .padding()
                    
                    ZStack {
                        WeightView(nn: currentViewingNN)
                        BiasView(nn: currentViewingNN)
                    }
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
            
            let layerSpacing = frameWidth / CGFloat(nn.neuralSize)
            let xAxisScale = 5 * (frameWidth / 500)

            ForEach(nn.UIIndexArray, id: \.self) { i in
                let baseX = layerSpacing * (CGFloat(i) + 1.5)
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
                
                // Line down the middle
                Path { path in
                    path.move(to: CGPoint(
                        x: baseX,
                        y: baseY
                    ))
                    
                    path.addLine(to: CGPoint(x: baseX, y: baseY + CGFloat(nn.layers[i].biases.count - 1) * ySegmentSpacing))
                }
                .stroke(
                    S_COL_ACC1,
                    style: StrokeStyle(lineWidth: 2, lineCap: .round, lineJoin: .round, miterLimit: 3, dash: [10], dashPhase: 0)
                )
            }
        }
    }
}

struct WeightView: View {
    let nn: NN
    
    var body: some View {
        ZStack {
            GeometryReader { geometry in
                let frameHeight = geometry.size.height
                let frameWidth = geometry.size.width
                
                let layerSpacing = frameWidth / CGFloat(nn.neuralSize)
                let xAxisScale = 5 * (frameWidth / 500)

                ForEach(nn.UIIndexArray, id: \.self) { i in
                    let baseXLeft = layerSpacing * (CGFloat(i) + 0.5)
                    let baseXRight = layerSpacing * (CGFloat(i + 1) + 0.5)
                    let ySegmentSpacing = frameHeight / CGFloat(nn.maxShape)
                    let baseYLeft = (frameHeight / 2) - ((CGFloat(nn.layers[i].weights.count) / 2) * ySegmentSpacing)
                    let baseYRight = (frameHeight / 2) - ((CGFloat(nn.layers[i].weights[0].count) / 2) * ySegmentSpacing)
                    
                    WeightLayerView(
                        nnLayer: nn.layers[i],
                        baseXLeft: baseXLeft,
                        baseXRight: baseXRight,
                        baseYLeft: baseYLeft,
                        baseYRight: baseYRight,
                        ySegmentSpacing: ySegmentSpacing
                    )
                }
            }
        }
        .drawingGroup()
    }
}


struct WeightLayerView: View {
    let nnLayer: NN.NNLayer
    let baseXLeft: CGFloat
    let baseXRight: CGFloat
    let baseYLeft: CGFloat
    let baseYRight: CGFloat
    let ySegmentSpacing: CGFloat
    
    var body: some View {
//                    for j in 0..<nn.layers[i].weights.count { // Column
        ForEach (nnLayer.UIColsArray, id: \.self) { j in
//                        for k in 0..<nn.layers[i].weights[j].count { // Row
            ForEach (nnLayer.UIRowsArray, id: \.self) { k in
                Path { path in
                        path.move(to: CGPoint(
                            x: baseXLeft,
                            y: baseYLeft + (CGFloat(j) * ySegmentSpacing)
                        ))
                        
                        path.addLine(to: CGPoint(
                            x: baseXRight,
                            y: baseYRight + (CGFloat(k) * ySegmentSpacing)
                        ))
                }
                .stroke(
                    S_COL_ACC2.opacity(nnLayer.weights[j][k] * 0.1),
                    style: StrokeStyle(lineWidth: 3, lineCap: .round, lineJoin: .round)
                )
            }
        }
    }
}
